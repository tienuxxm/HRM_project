namespace Domain.MembershipBenefits;

public interface IMembershipBenefitRepository
{
    void AddRange(List<MembershipBenefit> entities);
    void RemoveRange(List<MembershipBenefit> entities);
}