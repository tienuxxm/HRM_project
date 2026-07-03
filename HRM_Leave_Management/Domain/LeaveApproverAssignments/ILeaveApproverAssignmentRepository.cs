using System.Linq.Expressions;
using Domain.Abstractions;

namespace Domain.LeaveApproverAssignments;

public interface ILeaveApproverAssignmentRepository
{
    Task<LeaveApproverAssignment?> GetByIdAsync(LeaveApproverAssignmentId id, CancellationToken cancellationToken = default);

    void Add(LeaveApproverAssignment assignment);

    void Update(LeaveApproverAssignment assignment);

    void Remove(LeaveApproverAssignment assignment);

    IQueryable<LeaveApproverAssignment> GetEntitiesAsQueryable();

    Task<PagedList<LeaveApproverAssignment>> GetAllPaged(
        PagedQuery<LeaveApproverAssignment, LeaveApproverAssignmentId> request,
        IQueryable<LeaveApproverAssignment>? queryable = null);

    Task<bool> IsExistedAsync(
        Expression<Func<LeaveApproverAssignment, bool>> expression,
        CancellationToken cancellationToken = default);

    Task<List<LeaveApproverAssignment>?> GetAll(CancellationToken cancellationToken = default);
}
