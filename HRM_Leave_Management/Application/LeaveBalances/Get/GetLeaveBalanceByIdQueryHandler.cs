using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Abstractions.Role;
using Domain.Abstractions;
using Domain.Employees;
using Domain.LeaveBalances;
using Domain.LeaveRequests;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.LeaveBalances.Get;

internal sealed class GetLeaveBalanceByIdQueryHandler : IQueryHandler<GetLeaveBalanceByIdQuery, LeaveBalanceResponse>
{
    private readonly ILeaveBalanceRepository _leaveBalanceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;

    public GetLeaveBalanceByIdQueryHandler(
        ILeaveBalanceRepository leaveBalanceRepository,
        IUserRepository userRepository,
        IEmployeeRepository employeeRepository,
        ILeaveRequestRepository leaveRequestRepository,
        IUserContext userContext,
        IRoleService roleService)
    {
        _leaveBalanceRepository = leaveBalanceRepository;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _leaveRequestRepository = leaveRequestRepository;
        _userContext = userContext;
        _roleService = roleService;
    }

    public async Task<Result<LeaveBalanceResponse>> Handle(GetLeaveBalanceByIdQuery request, CancellationToken cancellationToken)
    {
        string identityId = _userContext.IdentityId;

        // 1. Kiểm tra các quyền
        var hasUpdatePermissionResult = await _roleService.checkRoleExist(identityId, "UPDATE_LEAVE_BALANCE", cancellationToken);
        bool hasUpdatePermission = hasUpdatePermissionResult.Value;

        var hasViewPermissionResult = await _roleService.checkRoleExist(identityId, "VIEW_LEAVE_BALANCE", cancellationToken);
        bool hasViewPermission = hasViewPermissionResult.Value;

        if (!hasUpdatePermission && !hasViewPermission)
        {
            return Result.Failure<LeaveBalanceResponse>(LeaveBalanceErrors.NotFound);
        }

        // 2. Lấy record Leave Balance kèm theo Employee và LeaveType
        var leaveBalanceId = new LeaveBalanceId(request.Id);
        var leaveBalance = await _leaveBalanceRepository.GetEntitiesAsQueryable()
            .Include(lb => lb.Employee)
            .Include(lb => lb.LeaveType)
            .FirstOrDefaultAsync(lb => lb.Id == leaveBalanceId && lb.IsActive, cancellationToken);

        if (leaveBalance == null)
        {
            return Result.Failure<LeaveBalanceResponse>(LeaveBalanceErrors.NotFound);
        }

        // 3. Nếu user chỉ có quyền VIEW_LEAVE_BALANCE (Self-view), bắt buộc phải khớp Employee của chính họ
        if (!hasUpdatePermission && hasViewPermission)
        {
            var user = await _userRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(u => u.IdentityId.Value == identityId, cancellationToken);

            if (user == null)
            {
                return Result.Failure<LeaveBalanceResponse>(LeaveBalanceErrors.NotFound);
            }

            var employee = await _employeeRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(e => e.UserId == user.Id && e.IsActive, cancellationToken);

            if (employee == null || leaveBalance.EmployeeId != employee.Id)
            {
                return Result.Failure<LeaveBalanceResponse>(LeaveBalanceErrors.NotFound);
            }
        }

        var pendingDays = await _leaveRequestRepository.GetEntitiesAsQueryable()
            .Where(lr => lr.Status == LeaveRequestStatus.Pending &&
                         lr.EmployeeId == leaveBalance.EmployeeId &&
                         lr.LeaveTypeId == leaveBalance.LeaveTypeId &&
                         lr.StartDate.Year == leaveBalance.Year)
            .SumAsync(lr => lr.Duration, cancellationToken);

        var response = new LeaveBalanceResponse
        {
            Id = leaveBalance.Id.Value,
            EmployeeId = leaveBalance.EmployeeId.Value,
            EmployeeName = leaveBalance.Employee?.FullName ?? "Unknown",
            EmployeeCode = leaveBalance.Employee?.EmployeeCode ?? "Unknown",
            LeaveTypeId = leaveBalance.LeaveTypeId.Value,
            LeaveTypeName = leaveBalance.LeaveType?.Name ?? "Unknown",
            LeaveTypeCode = leaveBalance.LeaveType?.Code ?? "Unknown",
            Year = leaveBalance.Year,
            AllocatedDays = leaveBalance.AllocatedDays,
            UsedDays = leaveBalance.UsedDays,
            PendingDays = pendingDays
        };

        return Result.Success(response);
    }
}
