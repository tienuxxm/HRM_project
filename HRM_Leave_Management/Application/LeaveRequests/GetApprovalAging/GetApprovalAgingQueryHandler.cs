using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Domain.Employees;
using Domain.LeaveRequests;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.LeaveRequests.GetApprovalAging;

/// <summary>
/// Dashboard W5 handler: Returns aging summary of pending approvals in the current user's scope.
/// Dynamic Approval Routing Source (Phase 7):
///   1. Resolve current user -> Employee (approver identity).
///   2. Scoped approver: Filter LeaveRequests assigned to current employee in LeaveRequestApprovalAssignment (AssignmentStatus == Assigned). EXCLUDES self-submitted requests.
///   3. Admin/HR (CanViewAllApprovals): Global view - returns ALL pending requests with active dynamic assignment (AssignmentStatus == Assigned). DOES NOT exclude self.
/// Aging buckets:
///   - Today: CreatedAt is today (age 0 days)
///   - 1-2 days: CreatedAt is 1-2 days ago
///   - 3+ days (Overdue): CreatedAt is 3+ days ago
/// Read-only. No DB mutation.
/// </summary>
internal sealed class GetApprovalAgingQueryHandler
    : IQueryHandler<GetApprovalAgingQuery, ApprovalAgingResult>
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveRequestApprovalAssignmentRepository _dynamicAssignmentRepository;
    private readonly IUserContext _userContext;

    public GetApprovalAgingQueryHandler(
        ILeaveRequestRepository leaveRequestRepository,
        IUserRepository userRepository,
        IEmployeeRepository employeeRepository,
        ILeaveRequestApprovalAssignmentRepository dynamicAssignmentRepository,
        IUserContext userContext)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _dynamicAssignmentRepository = dynamicAssignmentRepository;
        _userContext = userContext;
    }

    public async Task<Result<ApprovalAgingResult>> Handle(
        GetApprovalAgingQuery request, CancellationToken cancellationToken)
    {
        string identityId = _userContext.IdentityId;

        // Resolve current user -> employee
        var user = await _userRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(identityId), cancellationToken);

        if (user == null)
        {
            return Result.Success(new ApprovalAgingResult());
        }

        var employee = await _employeeRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(e => e.UserId == user.Id && e.IsActive, cancellationToken);

        // Scoped approver MUST have an active Employee record
        if (employee == null && !request.CanViewAllApprovals)
        {
            return Result.Success(new ApprovalAgingResult());
        }

        var now = DateTime.UtcNow;

        // Build base query: pending requests for ACTIVE employees only
        var baseQuery = _leaveRequestRepository.GetEntitiesAsQueryable()
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .Where(lr => lr.Status == LeaveRequestStatus.Pending)
            .Where(lr => lr.Employee.IsActive);

        if (!request.CanViewAllApprovals && employee != null)
        {
            // Scoped approver: filter pending requests assigned specifically to this employee in LeaveRequestApprovalAssignment, EXCLUDE self-submitted requests
            baseQuery = baseQuery
                .Where(lr => lr.EmployeeId != employee.Id)
                .Where(lr => _dynamicAssignmentRepository.GetEntitiesAsQueryable().Any(a =>
                    a.LeaveRequestId == lr.Id &&
                    a.AssignedApproverEmployeeId == employee.Id &&
                    a.AssignmentStatus == ApprovalAssignmentStatus.Assigned
                ));
        }
        else if (request.CanViewAllApprovals)
        {
            // Admin/HR global view: filter all pending requests with active dynamic assignment (AssignmentStatus == Assigned). DOES NOT exclude self.
            baseQuery = baseQuery.Where(lr => _dynamicAssignmentRepository.GetEntitiesAsQueryable().Any(a =>
                a.LeaveRequestId == lr.Id &&
                a.AssignmentStatus == ApprovalAssignmentStatus.Assigned
            ));
        }
        else
        {
            return Result.Success(new ApprovalAgingResult());
        }

        var pendingRequests = await baseQuery
            .OrderBy(lr => lr.CreatedAt)
            .ToListAsync(cancellationToken);

        // Calculate aging buckets
        int todayCount = 0;
        int oneToTwoDaysCount = 0;
        int overdueCount = 0;
        var overdueItems = new List<OverdueItem>();

        foreach (var lr in pendingRequests)
        {
            int ageDays = (int)(now - lr.CreatedAt).TotalDays;

            if (ageDays <= 0)
            {
                todayCount++;
            }
            else if (ageDays <= 2)
            {
                oneToTwoDaysCount++;
            }
            else
            {
                overdueCount++;
                overdueItems.Add(new OverdueItem
                {
                    Id = lr.Id.Value,
                    EmployeeName = lr.Employee.FullName,
                    LeaveTypeName = lr.LeaveType?.Name ?? "Unknown",
                    AgeDays = ageDays,
                    CreatedAt = lr.CreatedAt
                });
            }
        }

        // Top 3 oldest overdue
        var topOverdue = overdueItems
            .OrderByDescending(o => o.AgeDays)
            .Take(3)
            .ToList();

        return Result.Success(new ApprovalAgingResult
        {
            TodayCount = todayCount,
            OneToTwoDaysCount = oneToTwoDaysCount,
            OverdueCount = overdueCount,
            TopOverdue = topOverdue
        });
    }
}
