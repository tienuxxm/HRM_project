using Domain.Abstractions;
using Domain.MembershipClasses;
using Domain.Shared;

namespace Domain.MembershipBenefits;

public sealed class MembershipBenefit : Entity<MembershipBenefitId>
{
    private MembershipBenefit()
    {
    }

    private MembershipBenefit(MembershipBenefitId id, MembershipClassId membershipClassId, Description? description,
        Title title) : base(id)
    {
        MembershipClassId = membershipClassId;
        Description = description;
        Title = title;
    }

    public MembershipClassId MembershipClassId { get; private set; }
    public Title Title { get; private set; }
    public Description? Description { get; private set; }

    public static MembershipBenefit Create(MembershipClassId membershipClassId, Description? description, Title title)
    {
        return new MembershipBenefit(MembershipBenefitId.New, membershipClassId, description, title);
    }
}