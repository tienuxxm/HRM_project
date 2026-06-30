using Domain.MemberPointHistories;

namespace Infrastructure.Repositories;

internal sealed class MemberPointHistoryRepository : Repository<MemberPointHistory, MemberPointHistoryId>,
    IMemberPointHistoryRepository
{
    public MemberPointHistoryRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}