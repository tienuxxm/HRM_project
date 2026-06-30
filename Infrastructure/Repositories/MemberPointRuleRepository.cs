using Domain.MemberPointRules;

namespace Infrastructure.Repositories;

internal sealed class MemberPointRuleRepository: Repository<MemberPointRule, MemberPointRuleId>, IMemberPointRuleRepository
{
    public MemberPointRuleRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}