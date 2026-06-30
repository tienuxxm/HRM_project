using Domain.MemberActivities;

namespace Infrastructure.Repositories;

internal sealed class MemberActivityRepository : Repository<MemberActivity, MemberActivityId>, IMemberActivityRepository
{
    public MemberActivityRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}