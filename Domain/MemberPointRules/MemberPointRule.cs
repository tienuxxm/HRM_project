using Domain.Abstractions;
using Domain.MembershipClasses;
using Domain.Shared;

namespace Domain.MemberPointRules;

public class MemberPointRule : Entity<MemberPointRuleId>
{
    private MemberPointRule()
    {
    }

    public PointPerAmount PointPerAmount { get; private set; }
    public Money MinimumAmount { get; set; }

    private MemberPointRule(MemberPointRuleId id, PointPerAmount pointPerAmount, Money minimumAmount) : base(id)
    {
        PointPerAmount = pointPerAmount;
        MinimumAmount = minimumAmount;
    }

    public static MemberPointRule Create(PointPerAmount pointPerAmount, Money minimumAmount)
    {
        return new MemberPointRule(MemberPointRuleId.New, pointPerAmount, minimumAmount);
    }

    public int GetPoint(double amount)
    {
        return (int)Math.Floor(amount / (double)(MinimumAmount.Amount / PointPerAmount.Value));
    }
}