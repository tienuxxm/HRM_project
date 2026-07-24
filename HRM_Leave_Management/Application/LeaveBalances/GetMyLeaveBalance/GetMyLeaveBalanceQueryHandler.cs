using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Employees;
using Domain.LeaveBalances;
using Domain.LeaveRequests;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.LeaveBalances.GetMyLeaveBalance;

/// <summary>
/// Dashboard W6 handler: Returns the current employee's Annual Leave balance for the current year.
/// Business rule: only Annual Leave (LeaveType.Code == "AL" or Name contains "Annual").
/// If no Annual Leave balance exists, returns null (empty state).
/// PendingDays is calculated dynamically from pending leave requests.
/// Read-only. No DB mutation.
/// </summary>
internal sealed class GetMyLeaveBalanceQueryHandler : IQueryHandler<GetMyLeaveBalanceQuery, MyLeaveBalanceResult?>
{
    private readonly ILeaveBalanceRepository _leaveBalanceRepository;
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserContext _userContext;

    public GetMyLeaveBalanceQueryHandler(
        ILeaveBalanceRepository leaveBalanceRepository,
        ILeaveRequestRepository leaveRequestRepository,
        IUserRepository userRepository,
        IEmployeeRepository employeeRepository,
        IUserContext userContext)
    {
        _leaveBalanceRepository = leaveBalanceRepository;
        _leaveRequestRepository = leaveRequestRepository;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _userContext = userContext;
    }

    public async Task<Result<MyLeaveBalanceResult?>> Handle(GetMyLeaveBalanceQuery request, CancellationToken cancellationToken)
    {
        string identityId = _userContext.IdentityId;

        // Resolve current user -> employee
        var user = await _userRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(identityId), cancellationToken);

        if (user == null)
        {
            return Result.Success<MyLeaveBalanceResult?>(null);
        }

        var employee = await _employeeRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(e => e.UserId == user.Id && e.IsActive, cancellationToken);

        if (employee == null)
        {
            return Result.Success<MyLeaveBalanceResult?>(null);
        }

        int currentYear = DateTime.UtcNow.Year;

        // Find Annual Leave balance for current year
        // Strategy: match by LeaveType code "AL" or name containing "Annual" (case-insensitive)
        var balance = await _leaveBalanceRepository.GetEntitiesAsQueryable()
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.EmployeeId == employee.Id
                      && lb.IsActive
                      && lb.Year == currentYear
                      && lb.LeaveType != null
                      && (lb.LeaveType.Code == "AL"
                          || lb.LeaveType.Name.Contains("Annual")))
            .FirstOrDefaultAsync(cancellationToken);

        if (balance == null)
        {
            return Result.Success<MyLeaveBalanceResult?>(null);
        }

        // Calculate PendingDays from pending leave requests (same pattern as GetLeaveBalancesQueryHandler)
        decimal pendingDays = await _leaveRequestRepository.GetEntitiesAsQueryable()
            .Where(lr => lr.Status == LeaveRequestStatus.Pending
                      && lr.EmployeeId == employee.Id
                      && lr.LeaveTypeId == balance.LeaveTypeId
                      && lr.StartDate.Year == currentYear)
            .SumAsync(lr => lr.Duration, cancellationToken);

        return Result.Success<MyLeaveBalanceResult?>(new MyLeaveBalanceResult
        {
            AllocatedDays = balance.AllocatedDays,
            UsedDays = balance.UsedDays,
            PendingDays = pendingDays,
            LeaveTypeName = balance.LeaveType?.Name ?? "Annual Leave",
            Year = currentYear
        });
    }
}
