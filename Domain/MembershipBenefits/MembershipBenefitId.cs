namespace Domain.MembershipBenefits;

public record MembershipBenefitId(Guid Value)
{
    public static MembershipBenefitId New => new MembershipBenefitId(Guid.NewGuid());
}