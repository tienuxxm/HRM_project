namespace Domain.MemberDeviceTokens;

public interface IMemberDeviceTokenRepository
{
    void Add(MemberDeviceToken memberDeviceToken);

    void Update(MemberDeviceToken memberDeviceToken);
    Task<MemberDeviceToken?> GetByIdentityId(string identityId, CancellationToken cancellationToken = default);

    Task<bool> IsExisted(string token, CancellationToken cancellationToken = default);

    Task<List<MemberDeviceToken>> GetDeviceTokenAsync(List<string> identityIds,
        CancellationToken cancellationToken = default);

    Task<List<MemberDeviceToken>?> GetAll(
        CancellationToken cancellationToken = default);
}