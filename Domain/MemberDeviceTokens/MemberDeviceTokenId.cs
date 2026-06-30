namespace Domain.MemberDeviceTokens;

public record MemberDeviceTokenId(Guid Value)
{
    public static MemberDeviceTokenId New => new MemberDeviceTokenId(Guid.NewGuid());
}