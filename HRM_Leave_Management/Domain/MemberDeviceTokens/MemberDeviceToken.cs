using Domain.Abstractions;

namespace Domain.MemberDeviceTokens;

public class MemberDeviceToken : Entity<MemberDeviceTokenId>
{
    public string IdentityId { get; private set; }
    public string DeviceToken { get; private set; }

    private MemberDeviceToken()
    {
    }

    private MemberDeviceToken(MemberDeviceTokenId id, string identityId, string deviceToken) : base(id)
    {
        IdentityId = identityId;
        DeviceToken = deviceToken;
    }

    public static MemberDeviceToken Create(string identityId, string deviceToken)
    {
        return new MemberDeviceToken(MemberDeviceTokenId.New, identityId, deviceToken);
    }
}