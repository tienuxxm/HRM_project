using System.Linq.Expressions;
using Domain.Abstractions;

namespace Domain.LeaveTypes;

public interface ILeaveTypeRepository
{
    Task<LeaveType?> GetByIdAsync(LeaveTypeId id, CancellationToken cancellationToken = default);

    void Add(LeaveType leaveType);

    void Update(LeaveType leaveType);

    void Remove(LeaveType leaveType);

    IQueryable<LeaveType> GetEntitiesAsQueryable();

    Task<PagedList<LeaveType>> GetAllPaged(PagedQuery<LeaveType, LeaveTypeId> request,
        IQueryable<LeaveType>? queryable = null);

    Task<bool> IsExistedAsync(Expression<Func<LeaveType, bool>> expression,
        CancellationToken cancellationToken = default);

    Task<List<LeaveType>?> GetAll(CancellationToken cancellationToken = default);
}
