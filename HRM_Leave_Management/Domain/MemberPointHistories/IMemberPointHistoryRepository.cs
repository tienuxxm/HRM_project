using Domain.Abstractions;

namespace Domain.MemberPointHistories;

public interface IMemberPointHistoryRepository
{
    IQueryable<MemberPointHistory> GetEntitiesAsQueryable();

    Task<PagedList<MemberPointHistory>> GetAllPaged(PagedQuery<MemberPointHistory, MemberPointHistoryId> request,
        IQueryable<MemberPointHistory>? queryable = null);

    Task<MemberPointHistory?> GetByIdAsync(MemberPointHistoryId id, CancellationToken cancellationToken = default);

    void Add(MemberPointHistory memberPointHistory);
}