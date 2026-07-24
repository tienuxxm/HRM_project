using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Abstractions.Role;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Domain.Employees;
using Domain.LeaveRequests;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.LeaveRequests.GetMonthlyLeaveTrend;

/// <summary>
/// Dashboard W3 handler: Returns monthly leave request counts for the past 6 months.
/// Scope rules (Phase 8 Dynamic Routing & Deprecation):
///   - Admin/HR (UPDATE_LEAVE_APPROVER_ASSIGNMENT): all requests EXCEPT own
///   - Approver (APPROVE_LEAVE_REQUEST): requests where current employee is the assigned approver in LeaveRequestApprovalAssignment (excludes own)
///   - Employee (VIEW_LEAVE_REQUEST only): own requests only
/// Sole Source of Truth: LeaveRequestApprovalAssignment (Legacy LeaveApproverAssignment retired).
/// </summary>
internal sealed class GetMonthlyLeaveTrendQueryHandler
    : IQueryHandler<GetMonthlyLeaveTrendQuery, List<MonthlyLeaveTrendItem>>
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveRequestApprovalAssignmentRepository _approvalAssignmentRepository;
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;

    public GetMonthlyLeaveTrendQueryHandler(
        ILeaveRequestRepository leaveRequestRepository,
        IUserRepository userRepository,
        IEmployeeRepository employeeRepository,
        ILeaveRequestApprovalAssignmentRepository approvalAssignmentRepository,
        IUserContext userContext,
        IRoleService roleService)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _approvalAssignmentRepository = approvalAssignmentRepository;
        _userContext = userContext;
        _roleService = roleService;
    }

    public async Task<Result<List<MonthlyLeaveTrendItem>>> Handle(
        GetMonthlyLeaveTrendQuery request, CancellationToken cancellationToken)
    {
        string identityId = _userContext.IdentityId;

        var isAdminOrHR = (await _roleService.checkRoleExist(identityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken)).Value;
        var hasApprovePermission = (await _roleService.checkRoleExist(identityId, "APPROVE_LEAVE_REQUEST", cancellationToken)).Value;
        var hasViewPermission = (await _roleService.checkRoleExist(identityId, "VIEW_LEAVE_REQUEST", cancellationToken)).Value;

        if (!isAdminOrHR && !hasApprovePermission && !hasViewPermission)
        {
            return Result.Success(new List<MonthlyLeaveTrendItem>());
        }

        // Calculate 6-month boundary: first day of (current month - 5)
        var now = DateTime.UtcNow;
        var sixMonthsAgo = new DateOnly(now.Year, now.Month, 1).AddMonths(-5);

        var query = _leaveRequestRepository.GetEntitiesAsQueryable()
            .Include(lr => lr.Employee)
            .Where(lr => lr.StartDate >= sixMonthsAgo)
            .AsQueryable();

        // Resolve current employee
        var user = await _userRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(identityId), cancellationToken);

        if (user == null)
        {
            return Result.Success(new List<MonthlyLeaveTrendItem>());
        }

        var employee = await _employeeRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(e => e.UserId == user.Id && e.IsActive, cancellationToken);

        if (employee == null)
        {
            return Result.Success(new List<MonthlyLeaveTrendItem>());
        }

        // Apply dynamic approval routing scope rules
        if (isAdminOrHR)
        {
            query = query.Where(lr => lr.EmployeeId != employee.Id);
        }
        else if (hasApprovePermission)
        {
            query = query.Where(lr =>
                lr.EmployeeId != employee.Id &&
                _approvalAssignmentRepository.GetEntitiesAsQueryable().Any(a =>
                    a.LeaveRequestId == lr.Id &&
                    a.AssignedApproverEmployeeId == employee.Id &&
                    a.AssignmentStatus == ApprovalAssignmentStatus.Assigned
                )
            );
        }
        else
        {
            query = query.Where(lr => lr.EmployeeId == employee.Id);
        }

        // Group by year+month on StartDate
        var monthGroups = await query
            .GroupBy(lr => new { lr.StartDate.Year, lr.StartDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync(cancellationToken);

        // Build full 6-month series (fill gaps with 0)
        var monthNames = new[] { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        var result = new List<MonthlyLeaveTrendItem>();

        for (int i = 0; i < 6; i++)
        {
            var targetDate = sixMonthsAgo.AddMonths(i);
            int year = targetDate.Year;
            int month = targetDate.Month;
            int count = monthGroups.FirstOrDefault(g => g.Year == year && g.Month == month)?.Count ?? 0;

            result.Add(new MonthlyLeaveTrendItem
            {
                MonthLabel = monthNames[month],
                Year = year,
                Month = month,
                RequestCount = count
            });
        }

        return Result.Success(result);
    }
}
