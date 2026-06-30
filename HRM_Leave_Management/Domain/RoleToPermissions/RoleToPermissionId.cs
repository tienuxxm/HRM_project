namespace Domain.RoleToPermissions;

public record RoleToPermissionId(Guid Value)
{
    public static RoleToPermissionId New() => new(Guid.NewGuid());
}