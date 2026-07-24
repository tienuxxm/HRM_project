using Application.Abstractions.ApprovalRouting;
using Application.Abstractions.Authentication;
using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Application.WorkCalendars;
using Application.Response;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Domain.Employees;
using Domain.LeaveBalances;
using Domain.LeaveRequests;
using Domain.LeaveTypes;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.LeaveRequests.Create;

internal sealed class CreateLeaveRequestCommandHandler : ICommandHandler<CreateLeaveRequestCommand, BooleanResponse>
{
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveTypeRepository _leaveTypeRepository;
    private readonly ILeaveBalanceRepository _leaveBalanceRepository;
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IApprovalRouteResolverService _approvalRouteResolverService;
    private readonly ILeaveRequestApprovalAssignmentRepository _leaveRequestApprovalAssignmentRepository;
    private readonly IApprovalRouteAuditLogRepository _approvalRouteAuditLogRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IWorkCalendarService _workCalendarService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLeaveRequestCommandHandler(
        IUserContext userContext,
        IUserRepository userRepository,
        IEmployeeRepository employeeRepository,
        ILeaveTypeRepository leaveTypeRepository,
        ILeaveBalanceRepository leaveBalanceRepository,
        ILeaveRequestRepository leaveRequestRepository,
        IApprovalRouteResolverService approvalRouteResolverService,
        ILeaveRequestApprovalAssignmentRepository leaveRequestApprovalAssignmentRepository,
        IApprovalRouteAuditLogRepository approvalRouteAuditLogRepository,
        IDateTimeProvider dateTimeProvider,
        IWorkCalendarService workCalendarService,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _leaveTypeRepository = leaveTypeRepository;
        _leaveBalanceRepository = leaveBalanceRepository;
        _leaveRequestRepository = leaveRequestRepository;
        _approvalRouteResolverService = approvalRouteResolverService;
        _leaveRequestApprovalAssignmentRepository = leaveRequestApprovalAssignmentRepository;
        _approvalRouteAuditLogRepository = approvalRouteAuditLogRepository;
        _dateTimeProvider = dateTimeProvider;
        _workCalendarService = workCalendarService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch Employee from UserContext IdentityId
        var identityId = _userContext.IdentityId;
        var user = await _userRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(identityId), cancellationToken);
        if (user == null)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.EmployeeNotFound);
        }

        var employee = await _employeeRepository.GetEntitiesAsQueryable()
            .Include(e => e.Position)
            .FirstOrDefaultAsync(e => e.UserId == user.Id && e.IsActive, cancellationToken);
        if (employee == null)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.EmployeeNotFound);
        }

        var employeeId = employee.Id;

        // 2. Validate Date order
        if (request.StartDate > request.EndDate)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.DateOrderInvalid);
        }

        // 3. Validate past dates not allowed
        var utcNow = _dateTimeProvider.UtcNow;
        var businessToday = DateOnly.FromDateTime(utcNow);
        if (request.StartDate < businessToday)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.PastDateNotAllowed);
        }

        // 4. Validate LeaveType active
        var leaveTypeId = new LeaveTypeId(request.LeaveTypeId);
        var leaveType = await _leaveTypeRepository.GetByIdAsync(leaveTypeId, cancellationToken);
        if (leaveType == null || !leaveType.IsActive)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.LeaveTypeNotFound);
        }

        // 5. Calculate Duration via WorkCalendarService
        if (request.StartDate.Year != request.EndDate.Year)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.CrossYearNotAllowed);
        }

        if (request.StartDate == request.EndDate)
        {
            if (request.StartDayPart != request.EndDayPart)
            {
                return Result.Failure<BooleanResponse>(LeaveRequestErrors.DayPartMismatch);
            }
        }

        var durationResult = await _workCalendarService.CalculateLeaveDurationAsync(
            request.StartDate,
            request.EndDate,
            request.StartDayPart,
            request.EndDayPart,
            cancellationToken);

        if (durationResult.IsFailure)
        {
            return Result.Failure<BooleanResponse>(durationResult.Error);
        }

        decimal duration = durationResult.Value;

        if (duration == 0)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.OnlyNonWorkingDays);
        }

        // 6. Check leave date overlap
        var isOverlapped = await _leaveRequestRepository.IsExistedAsync(lr =>
            lr.EmployeeId == employeeId &&
            (lr.Status == LeaveRequestStatus.Pending || lr.Status == LeaveRequestStatus.Approved) &&
            lr.StartDate <= request.EndDate &&
            lr.EndDate >= request.StartDate,
            cancellationToken);

        if (isOverlapped)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.OverlapDetected);
        }

        // 7. Check Leave Balance
        int targetYear = request.StartDate.Year;
        var leaveBalance = await _leaveBalanceRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(lb =>
                lb.EmployeeId == employeeId &&
                lb.LeaveTypeId == leaveTypeId &&
                lb.Year == targetYear &&
                lb.IsActive,
                cancellationToken);

        if (leaveBalance == null)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.NoLeaveBalance);
        }

        // 8. Calculate Pending Duration
        var pendingDuration = await _leaveRequestRepository.GetEntitiesAsQueryable()
            .Where(lr =>
                lr.EmployeeId == employeeId &&
                lr.LeaveTypeId == leaveTypeId &&
                lr.Status == LeaveRequestStatus.Pending &&
                lr.StartDate.Year == targetYear)
            .SumAsync(lr => lr.Duration, cancellationToken);

        decimal availableDays = leaveBalance.AllocatedDays - leaveBalance.UsedDays - pendingDuration;

        if (duration > availableDays)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.InsufficientBalance);
        }

        // 9. Resolve Approver via ApprovalRouteResolverService
        var resolutionResult = await _approvalRouteResolverService.ResolveApproverAsync(employee, cancellationToken);
        if (!resolutionResult.IsSuccess)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.ApprovalRouteNotConfigured);
        }

        // Rule 4: Handle AutoApprove for Terminal Company-Level Approver
        if (resolutionResult.IsAutoApproved)
        {
            var autoLeaveRequest = LeaveRequest.Create(
                employeeId,
                leaveTypeId,
                request.StartDate,
                request.EndDate,
                request.StartDayPart,
                request.EndDayPart,
                duration,
                request.Reason,
                utcNow);

            autoLeaveRequest.AutoApprove(utcNow);
            leaveBalance.AddUsedDays(duration);

            var autoAuditLog = ApprovalRouteAuditLog.LogAction(
                autoLeaveRequest.Id,
                assignmentId: null,
                previousApproverId: null,
                newApproverId: null,
                ApprovalRouteAuditActionType.AutoApproved,
                oldStatus: null,
                newStatus: LeaveRequestStatus.Approved.ToString(),
                reasonCode: "ConfiguredTerminalApproverAutoApproved",
                createdByUserId: user.Id.Value,
                note: "Leave request auto-approved for terminal approver per routing configuration.");

            _leaveRequestRepository.Add(autoLeaveRequest);
            _leaveBalanceRepository.Update(leaveBalance);
            _approvalRouteAuditLogRepository.Add(autoAuditLog);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new BooleanResponse
            {
                Result = true,
                Message = "Leave request auto-approved successfully."
            });
        }

        // 10. Standard Routing Path: Create LeaveRequest (Pending) and routing metadata
        var leaveRequest = LeaveRequest.Create(
            employeeId,
            leaveTypeId,
            request.StartDate,
            request.EndDate,
            request.StartDayPart,
            request.EndDayPart,
            duration,
            request.Reason,
            utcNow);

        var assignedApprover = resolutionResult.AssignedApprover!;
        var reason = resolutionResult.CandidateId == null
            ? ApprovalAssignmentReason.SpecificEmployeeOverride
            : (resolutionResult.PriorityOrder == 1
                ? ApprovalAssignmentReason.DirectLevelMatch
                : ApprovalAssignmentReason.SuperiorLevelEscalated);

        var assignment = LeaveRequestApprovalAssignment.CreateAssigned(
            leaveRequest.Id,
            assignedApprover.Id,
            reason,
            resolutionResult.PolicyId,
            resolutionResult.RuleId,
            resolutionResult.CandidateId,
            resolutionResult.LevelAssignmentId);

        var auditLog = ApprovalRouteAuditLog.LogAction(
            leaveRequest.Id,
            assignment.Id,
            previousApproverId: null,
            newApproverId: assignedApprover.Id,
            ApprovalRouteAuditActionType.Created,
            oldStatus: null,
            newStatus: ApprovalAssignmentStatus.Assigned.ToString(),
            reasonCode: reason.ToString(),
            createdByUserId: user.Id.Value,
            note: $"Initial routing assigned to approver employee ID {assignedApprover.Id.Value} (Priority {resolutionResult.PriorityOrder}).");

        _leaveRequestRepository.Add(leaveRequest);
        _leaveRequestApprovalAssignmentRepository.Add(assignment);
        _approvalRouteAuditLogRepository.Add(auditLog);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BooleanResponse
        {
            Result = true,
            Message = "Leave request created successfully."
        });
    }
}
