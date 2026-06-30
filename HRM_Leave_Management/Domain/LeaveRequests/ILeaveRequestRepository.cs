using System.Linq.Expressions;
using Domain.Abstractions;

namespace Domain.LeaveRequests;

public interface ILeaveRequestRepository
{
    Task<LeaveRequest?> GetByIdAsync(LeaveRequestId id, CancellationToken cancellationToken = default);

    void Add(LeaveRequest leaveRequest);

    void Update(LeaveRequest leaveRequest);

    void Remove(LeaveRequest leaveRequest);

    IQueryable<LeaveRequest> GetEntitiesAsQueryable();

    Task<PagedList<LeaveRequest>> GetAllPaged(PagedQuery<LeaveRequest, LeaveRequestId> request,
        IQueryable<LeaveRequest>? queryable = null);

    Task<bool> IsExistedAsync(Expression<Func<LeaveRequest, bool>> expression,
        CancellationToken cancellationToken = default);
}
