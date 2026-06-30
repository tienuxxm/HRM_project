using Domain.MembershipBenefits;

namespace Infrastructure.Repositories;

internal sealed class MembershipBenefitRepository : Repository<MembershipBenefit, MembershipBenefitId>,
    IMembershipBenefitRepository
{
    public MembershipBenefitRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}