using Application.Abstractions.Authentication;
using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Application.WorkCalendars;
using Application.Response;
using Domain.Abstractions;
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
        _dateTimeProvider = dateTimeProvider;
        _workCalendarService = workCalendarService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        // 1. Lấy thông tin Employee từ UserContext.IdentityId
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
        bool isCeo = employee.Position != null && employee.Position.Code == "CEO";

        // 2. Validate ngày hợp lệ (V-2)
        if (request.StartDate > request.EndDate)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.DateOrderInvalid);
        }

        // 3. Validate không xin nghỉ quá khứ (V-3)
        var utcNow = _dateTimeProvider.UtcNow;
        var businessToday = DateOnly.FromDateTime(utcNow);
        if (request.StartDate < businessToday)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.PastDateNotAllowed);
        }

        // 4. Validate LeaveType tồn tại & active
        var leaveTypeId = new LeaveTypeId(request.LeaveTypeId);
        var leaveType = await _leaveTypeRepository.GetByIdAsync(leaveTypeId, cancellationToken);
        if (leaveType == null || !leaveType.IsActive)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.LeaveTypeNotFound);
        }

        // 5. Tính toán Duration thông qua WorkCalendarService
        // Đồng thời kiểm tra không cho phép đơn nghỉ bắc qua nhiều năm calendar.
        if (request.StartDate.Year != request.EndDate.Year)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.CrossYearNotAllowed);
        }

        if (request.StartDate == request.EndDate)
        {
            // Yêu cầu cùng ngày phải có StartDayPart == EndDayPart
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

        // 6. Kiểm tra trùng lịch nghỉ (V-4)
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

        // 7. Kiểm tra Leave Balance theo năm của ngày bắt đầu nghỉ (V-1)
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

        // 8. Tính số ngày phép đang ở trạng thái Pending trong targetYear (V-6)
        var pendingDuration = await _leaveRequestRepository.GetEntitiesAsQueryable()
            .Where(lr =>
                lr.EmployeeId == employeeId &&
                lr.LeaveTypeId == leaveTypeId &&
                lr.Status == LeaveRequestStatus.Pending &&
                lr.StartDate.Year == targetYear)
            .SumAsync(lr => lr.Duration, cancellationToken);

        decimal availableDays = leaveBalance.AllocatedDays - leaveBalance.UsedDays - pendingDuration;

        // Kiểm tra số dư khả dụng (V-6)
        if (duration > availableDays)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.InsufficientBalance);
        }

        // 9. Tạo đơn nghỉ phép mới
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

        if (isCeo)
        {
            leaveRequest.SetApprovedForCeo(utcNow);
            leaveBalance.AddUsedDays(duration);
            _leaveBalanceRepository.Update(leaveBalance);
        }

        _leaveRequestRepository.Add(leaveRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BooleanResponse
        {
            Result = true,
            Message = "Leave request created successfully."
        });
    }
}
