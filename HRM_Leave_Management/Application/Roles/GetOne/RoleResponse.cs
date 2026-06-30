using Application.Permissions.GetAll;
using Application.Users.GetOne;

namespace Application.Roles.GetOne;

public sealed class RoleResponse
{
    public Guid Id { get; init; }

    public string ResourceName { get; init; }

    public string DisplayName { get; init; }

    public DateTime CreatedDate { get; init; }

    public List<UserResponse>? Users { get; set; }

    public List<PermissionResponse>? Permissions { get; set; }

    public string PermissionsResponse =>
        Permissions is null ? "" : string.Join(",", Permissions.Select(x => x.DisplayName));
}