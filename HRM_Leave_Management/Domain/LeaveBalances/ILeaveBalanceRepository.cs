using System.Linq.Expressions;
using Domain.Abstractions;

namespace Domain.LeaveBalances;

public interface ILeaveBalanceRepository
{
    Task<LeaveBalance?> GetByIdAsync(LeaveBalanceId id, CancellationToken cancellationToken = default);

    void Add(LeaveBalance leaveBalance);

    void Update(LeaveBalance leaveBalance);

    void Remove(LeaveBalance leaveBalance);

    IQueryable<LeaveBalance> GetEntitiesAsQueryable();

    Task<PagedList<LeaveBalance>> GetAllPaged(PagedQuery<LeaveBalance, LeaveBalanceId> request,
        IQueryable<LeaveBalance>? queryable = null);

    Task<bool> IsExistedAsync(Expression<Func<LeaveBalance, bool>> expression,
        CancellationToken cancellationToken = default);
}
