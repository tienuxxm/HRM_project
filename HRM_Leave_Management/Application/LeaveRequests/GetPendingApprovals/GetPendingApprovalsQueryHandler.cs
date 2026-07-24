using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Domain.Employees;
using Domain.LeaveRequests;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.LeaveRequests.GetPendingApprovals;

/// <summary>
/// Dashboard W4 handler: Returns up to 4 pending leave requests in the current user's approval scope.
/// Dynamic Approval Routing Source (Phase 7):
///   1. Resolve current user -> Employee (approver identity).
///   2. Scoped approver: Filter LeaveRequests assigned to current employee in LeaveRequestApprovalAssignment (AssignmentStatus == Assigned). EXCLUDES self-submitted requests.
///   3. Admin/HR (CanViewAllApprovals): Global view - returns ALL pending requests with active dynamic assignment (AssignmentStatus == Assigned). DOES NOT exclude self.
///   4. Order by CreatedAt ASC (oldest first - FIFO queue).
/// Read-only. No DB mutation.
/// </summary>
internal sealed class GetPendingApprovalsQueryHandler
    : IQueryHandler<GetPendingApprovalsQuery, List<PendingApprovalItem>>
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveRequestApprovalAssignmentRepository _dynamicAssignmentRepository;
    private readonly IUserContext _userContext;

    public GetPendingApprovalsQueryHandler(
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

    public async Task<Result<List<PendingApprovalItem>>> Handle(
        GetPendingApprovalsQuery request, CancellationToken cancellationToken)
    {
        string identityId = _userContext.IdentityId;

        // Resolve current user -> employee
        var user = await _userRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(identityId), cancellationToken);

        if (user == null)
        {
            return Result.Success(new List<PendingApprovalItem>());
        }

        var employee = await _employeeRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(e => e.UserId == user.Id && e.IsActive, cancellationToken);

        // Scoped approver MUST have an active Employee record
        if (employee == null && !request.CanViewAllApprovals)
        {
            return Result.Success(new List<PendingApprovalItem>());
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
            return Result.Success(new List<PendingApprovalItem>());
        }

        var items = await baseQuery
            .OrderBy(lr => lr.CreatedAt) // FIFO: oldest first
            .Take(4)
            .Select(lr => new PendingApprovalItem
            {
                Id = lr.Id.Value,
                EmployeeName = lr.Employee.FullName,
                EmployeeCode = lr.Employee.EmployeeCode,
                LeaveTypeName = lr.LeaveType != null ? lr.LeaveType.Name : "Unknown",
                StartDate = lr.StartDate,
                EndDate = lr.EndDate,
                Duration = lr.Duration,
                CreatedAt = lr.CreatedAt,
                PendingDays = (int)(now - lr.CreatedAt).TotalDays
            })
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}
