using Domain.ApprovalRouting;
using Domain.Employees;
using Domain.LeaveRequests;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class LeaveRequestApprovalAssignmentRepository : Repository<LeaveRequestApprovalAssignment, LeaveRequestApprovalAssignmentId>, ILeaveRequestApprovalAssignmentRepository
{
    public LeaveRequestApprovalAssignmentRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<LeaveRequestApprovalAssignment?> GetByLeaveRequestIdAsync(LeaveRequestId leaveRequestId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<LeaveRequestApprovalAssignment>()
            .Include(a => a.AssignedApprover)
            .Include(a => a.LeaveRequest)
                .ThenInclude(lr => lr!.Employee)
            .Include(a => a.LeaveRequest)
                .ThenInclude(lr => lr!.LeaveType)
            .FirstOrDefaultAsync(a => a.LeaveRequestId == leaveRequestId, cancellationToken);
    }

    public async Task<List<LeaveRequestApprovalAssignment>> GetPendingAssignmentsByApproverAsync(EmployeeId approverEmployeeId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<LeaveRequestApprovalAssignment>()
            .Include(a => a.LeaveRequest)
                .ThenInclude(lr => lr!.Employee)
            .Include(a => a.LeaveRequest)
                .ThenInclude(lr => lr!.LeaveType)
            .Where(a => a.AssignedApproverEmployeeId == approverEmployeeId
                        && a.AssignmentStatus == ApprovalAssignmentStatus.Assigned
                        && a.LeaveRequest != null
                        && a.LeaveRequest.Status == LeaveRequestStatus.Pending)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LeaveRequestApprovalAssignment>> GetPendingAssignmentsByApproversAsync(IEnumerable<EmployeeId> approverEmployeeIds, CancellationToken cancellationToken = default)
    {
        var approverIdsList = approverEmployeeIds.ToList();
        return await DbContext.Set<LeaveRequestApprovalAssignment>()
            .Include(a => a.LeaveRequest)
                .ThenInclude(lr => lr!.Employee)
            .Include(a => a.LeaveRequest)
                .ThenInclude(lr => lr!.LeaveType)
            .Where(a => a.AssignedApproverEmployeeId != null
                        && approverIdsList.Contains(a.AssignedApproverEmployeeId)
                        && a.AssignmentStatus == ApprovalAssignmentStatus.Assigned
                        && a.LeaveRequest != null
                        && a.LeaveRequest.Status == LeaveRequestStatus.Pending)
            .ToListAsync(cancellationToken);
    }
}
