namespace Domain.MemberActivities;

public record MemberActivityId(Guid Value)
{
    public static MemberActivityId New => new MemberActivityId(Guid.NewGuid());
}