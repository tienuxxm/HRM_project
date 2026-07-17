using Application.Abstractions.Messaging;
using Application.Abstractions.Authentication;
using Application.Abstractions.Clock;
using Domain.Abstractions;
using Domain.WorkCalendars;
using Domain.LeaveRequests;
using Domain.LeaveBalances;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.WorkCalendars.SaveManualCalendarDay;

internal sealed class SaveManualCalendarDayCommandHandler : ICommandHandler<SaveManualCalendarDayCommand>
{
    private readonly IWorkCalendarDayRepository _workCalendarDayRepository;
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly ILeaveRequestRecalculationAuditRepository _leaveRequestRecalculationAuditRepository;
    private readonly ILeaveBalanceRepository _leaveBalanceRepository;
    private readonly IWorkCalendarService _workCalendarService;
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public SaveManualCalendarDayCommandHandler(
        IWorkCalendarDayRepository workCalendarDayRepository,
        ILeaveRequestRepository leaveRequestRepository,
        ILeaveRequestRecalculationAuditRepository leaveRequestRecalculationAuditRepository,
        ILeaveBalanceRepository leaveBalanceRepository,
        IWorkCalendarService workCalendarService,
        IUserContext userContext,
        IUserRepository userRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _workCalendarDayRepository = workCalendarDayRepository;
        _leaveRequestRepository = leaveRequestRepository;
        _leaveRequestRecalculationAuditRepository = leaveRequestRecalculationAuditRepository;
        _leaveBalanceRepository = leaveBalanceRepository;
        _workCalendarService = workCalendarService;
        _userContext = userContext;
        _userRepository = userRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SaveManualCalendarDayCommand request, CancellationToken cancellationToken)
    {
        // 0. Validate not in the past
        var today = DateOnly.FromDateTime(_dateTimeProvider.ToVnTime(_dateTimeProvider.UtcNow));
        if (request.Date < today)
        {
            return Result.Failure(WorkCalendarErrors.PastEditingNotAllowed);
        }

        // 1. Get current logged-in user
        var identityId = _userContext.IdentityId;
        var user = await _userRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(identityId), cancellationToken);

        if (user == null)
        {
            return Result.Failure(new Error("SaveManual.UserNotFound", "Current user not found."));
        }

        // 2. Parse Enum values
        if (!Enum.TryParse<CalendarDayType>(request.DayType, true, out var inputDayType))
        {
            return Result.Failure(new Error("SaveManual.InvalidDayType", $"Invalid DayType value '{request.DayType}'."));
        }

        if (!Enum.TryParse<WorkShiftType>(request.WorkShift, true, out var inputWorkShift))
        {
            return Result.Failure(new Error("SaveManual.InvalidWorkShift", $"Invalid WorkShift value '{request.WorkShift}'."));
        }

        // Validate consistency rules
        if ((inputDayType == CalendarDayType.PublicHoliday || inputDayType == CalendarDayType.CompanyCustomNonWorkingDay)
            && inputWorkShift != WorkShiftType.None)
        {
            return Result.Failure(new Error("SaveManual.InvalidCombination", $"DayType '{inputDayType}' must have WorkShift set to 'None'. Got '{inputWorkShift}'."));
        }
        else if ((inputDayType == CalendarDayType.WorkingSaturdayOverride || inputDayType == CalendarDayType.StandardWorkingDayOverride)
            && inputWorkShift == WorkShiftType.None)
        {
            return Result.Failure(new Error("SaveManual.InvalidCombination", $"DayType '{inputDayType}' cannot have WorkShift set to 'None'."));
        }

        // 3. Begin Transaction
        using var transaction = _unitOfWork.BeginTransaction();
        try
        {
            // 4. Create or Update WorkCalendarDay
            var existingDay = await _workCalendarDayRepository.GetByDateAsync(request.Date, cancellationToken);
            if (existingDay != null)
            {
                existingDay.Update(inputDayType, inputWorkShift, request.Description);
                existingDay.SetActive(request.IsActive);
                _workCalendarDayRepository.Update(existingDay);
            }
            else
            {
                var newDay = WorkCalendarDay.Create(
                    request.Date,
                    inputDayType,
                    inputWorkShift,
                    request.Description,
                    user.Id.Value);
                newDay.SetActive(request.IsActive);
                await _workCalendarDayRepository.AddAsync(newDay, cancellationToken);
            }

            // SaveChanges first so the recalculated leave duration reads the updated calendar day from DB
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5. Query affected leave requests
            var affectedLeaveRequests = await _leaveRequestRepository.GetEntitiesAsQueryable()
                .Where(lr => (lr.Status == LeaveRequestStatus.Approved || lr.Status == LeaveRequestStatus.Pending)
                             && lr.StartDate <= request.Date
                             && lr.EndDate >= request.Date)
                .ToListAsync(cancellationToken);

            foreach (var lr in affectedLeaveRequests)
            {
                var oldStatus = lr.Status;
                var oldDuration = lr.Duration;
                var oldProcessedBy = lr.ProcessedBy;
                var oldProcessedAt = lr.ProcessedAt;
                var oldComment = lr.Comment;

                var durationResult = await _workCalendarService.CalculateLeaveDurationAsync(
                    lr.StartDate,
                    lr.EndDate,
                    lr.StartDayPart,
                    lr.EndDayPart,
                    cancellationToken);

                if (durationResult.IsSuccess)
                {
                    var newDuration = durationResult.Value;

                    if (newDuration != oldDuration)
                    {
                        if (oldStatus == LeaveRequestStatus.Approved)
                        {
                            var leaveBalance = await _leaveBalanceRepository.GetEntitiesAsQueryable()
                                .FirstOrDefaultAsync(lb => lb.EmployeeId == lr.EmployeeId
                                                           && lb.LeaveTypeId == lr.LeaveTypeId
                                                           && lb.Year == lr.StartDate.Year,
                                                     cancellationToken);
                            if (leaveBalance == null)
                            {
                                throw new InvalidOperationException($"Leave balance not found for employee {lr.EmployeeId}, leave type {lr.LeaveTypeId}, year {lr.StartDate.Year}");
                            }

                            leaveBalance.ReturnUsedDays(oldDuration);
                            _leaveBalanceRepository.Update(leaveBalance);

                            lr.ReopenToPending(newDuration);
                        }
                        else if (oldStatus == LeaveRequestStatus.Pending)
                        {
                            lr.UpdateDurationOnly(newDuration);
                        }

                        _leaveRequestRepository.Update(lr);

                        var auditStatus = newDuration == 0.0m
                            ? RecalculationAuditStatus.NeedsEmployeeRevision
                            : RecalculationAuditStatus.Success;

                        var errorMessage = newDuration == 0.0m
                            ? "New duration is 0.0. Needs employee revision or cancellation."
                            : null;

                        var audit = LeaveRequestRecalculationAudit.Create(
                            null, // null batchId for manual changes
                            lr,
                            oldStatus,
                            lr.Status,
                            oldDuration,
                            newDuration,
                            auditStatus,
                            oldProcessedBy,
                            oldProcessedAt,
                            oldComment,
                            errorMessage);

                        await _leaveRequestRecalculationAuditRepository.AddAsync(audit, cancellationToken);
                    }
                }
                else
                {
                    var audit = LeaveRequestRecalculationAudit.Create(
                        null, // null batchId for manual changes
                        lr,
                        oldStatus,
                        oldStatus,
                        oldDuration,
                        oldDuration,
                        RecalculationAuditStatus.Failed,
                        oldProcessedBy,
                        oldProcessedAt,
                        oldComment,
                        durationResult.Error.Name);

                    await _leaveRequestRecalculationAuditRepository.AddAsync(audit, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            transaction.Commit();

            return Result.Success();
        }
        catch (Exception ex)
        {
            try
            {
                transaction.Rollback();
            }
            catch
            {
                // Ignore rollback exception
            }

            return Result.Failure(new Error("SaveManual.Failed", $"Failed to save manual calendar day: {ex.Message}"));
        }
    }
}
