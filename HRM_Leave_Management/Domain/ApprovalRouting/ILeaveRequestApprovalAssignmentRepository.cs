using Domain.Employees;
using Domain.LeaveRequests;

namespace Domain.ApprovalRouting;

public interface ILeaveRequestApprovalAssignmentRepository
{
    Task<LeaveRequestApprovalAssignment?> GetByIdAsync(LeaveRequestApprovalAssignmentId id, CancellationToken cancellationToken = default);
    Task<LeaveRequestApprovalAssignment?> GetByLeaveRequestIdAsync(LeaveRequestId leaveRequestId, CancellationToken cancellationToken = default);
    Task<List<LeaveRequestApprovalAssignment>> GetPendingAssignmentsByApproverAsync(EmployeeId approverEmployeeId, CancellationToken cancellationToken = default);
    Task<List<LeaveRequestApprovalAssignment>> GetPendingAssignmentsByApproversAsync(IEnumerable<EmployeeId> approverEmployeeIds, CancellationToken cancellationToken = default);
    void Add(LeaveRequestApprovalAssignment assignment);
    void Update(LeaveRequestApprovalAssignment assignment);
    void Remove(LeaveRequestApprovalAssignment assignment);
    IQueryable<LeaveRequestApprovalAssignment> GetEntitiesAsQueryable();
}
