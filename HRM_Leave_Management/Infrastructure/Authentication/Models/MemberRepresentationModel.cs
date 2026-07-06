using Domain.Members;
using Domain.Users;

namespace Infrastructure.Authentication.Models;

public sealed class MemberRepresentationModel
{
    public Dictionary<string, string> Access { get; set; }

    public Dictionary<string, List<string>> Attributes { get; set; }

    public Dictionary<string, string> ClientRoles { get; set; }

    public long? CreatedTimestamp { get; set; }

    public CredentialRepresentationModel[] Credentials { get; set; }

    public string[] DisableableCredentialTypes { get; set; }

    public string Email { get; set; }

    public bool? EmailVerified { get; set; }

    public bool? Enabled { get; set; }

    public string FederationLink { get; set; }

    public string Id { get; set; }

    public string[] Groups { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public int? NotBefore { get; set; }

    public string Origin { get; set; }

    public string[] RealmRoles { get; set; }

    public string[] RequiredActions { get; set; }

    public string Self { get; set; }

    public string ServiceAccountClientId { get; set; }

    public string Username { get; set; }

    internal static MemberRepresentationModel FromUser(Member member)
    {
        return new MemberRepresentationModel
        {
            FirstName = member.FirstName.Value,
            LastName = member.LastName.Value,
            Email = member.Email.Value,
            Username = member.Email.Value,
            Enabled = true,
            EmailVerified = true,
            CreatedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Attributes = new Dictionary<string, List<string>>(),
            RequiredActions = Array.Empty<string>()
        };
    }

    internal static MemberRepresentationModel FromUser(User user)
    {
        return new MemberRepresentationModel
        {
            FirstName = user.Name.Value,
            LastName = user.Name.Value,
            Email = user.Email.Value,
            Username = user.Username.Value,
            Enabled = true,
            EmailVerified = true,
            CreatedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Attributes = new Dictionary<string, List<string>>(),
            RequiredActions = Array.Empty<string>()
        };
    }
}