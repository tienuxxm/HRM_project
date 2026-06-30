namespace Domain.MemberPointRules;

public record MemberPointRuleId(Guid Value)
{
    public static MemberPointRuleId New => new MemberPointRuleId(Guid.NewGuid());
}