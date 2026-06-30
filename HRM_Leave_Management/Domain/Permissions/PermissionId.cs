namespace Domain.Permissions;

public record PermissionId(Guid Value)
{
    public static PermissionId New() => new(Guid.NewGuid());
}