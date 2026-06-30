namespace Domain.Roles;

public record RoleId(Guid Value)
{
    public static RoleId New() => new(Guid.NewGuid());
}