using Domain.Abstractions;
using Domain.UserToRoles;

namespace Domain.Users;

public class User : Entity<UserId>
{
    private User(UserId id, Name name, Username username, Email? email,
        PhoneNumber? phoneNumber, List<UserToRole>? roles, DateTime createdAt)
        : base(id)
    {
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
        Username = username;
        Roles = roles;
        CreatedAt = createdAt;
        IsDeleted = false;
    }

    private User()
    {
    }

    public IdentityId IdentityId { get; private set; }

    public Email Email { get; private set; }

    public Name Name { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }

    public Username Username { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public bool? IsDeleted { get; private set; }

    public List<UserToRole>? Roles { get; private set; }

    public void SetIdentityId(string identityId)
    {
        IdentityId = new IdentityId(identityId);
    }

    public void Delete()
    {
        IsDeleted = true;
    }

    public static User Create(Name name, Email? email, PhoneNumber? phoneNumber,
        Username username, List<UserToRole>? roles, DateTime createdAt)
    {
        return new User(UserId.New(), name, username, email, phoneNumber, roles, createdAt);
    }

    public void Update(
        string? name,
        string? email,
        string? phoneNumber)
    {
        Name = !string.IsNullOrEmpty(name) ? new Name(name) : Name;
        Email = email != null ? new Email(email) : Email;
        PhoneNumber = phoneNumber != null ? new PhoneNumber(phoneNumber) : PhoneNumber;
    }

    public void UpdateRoles(List<UserToRole> roles)
    {
        Roles = roles;
    }
}