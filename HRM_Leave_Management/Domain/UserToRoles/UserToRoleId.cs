namespace Domain.UserToRoles;

public record UserToRoleId(Guid Value)
{
    public static UserToRoleId New() => new(Guid.NewGuid());
}