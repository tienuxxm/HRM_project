namespace Domain.MembershipClasses;

public record MembershipClassId(Guid Value)
{
    public static MembershipClassId New => new(Guid.NewGuid());
}