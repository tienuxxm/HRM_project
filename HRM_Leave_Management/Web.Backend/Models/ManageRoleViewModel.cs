using Application.Permissions.GetAll;
using Application.Roles.GetOne;

namespace Web.Backend.Models;

public class ManageRoleViewModel
{
    public ManageRoleModel ManageRoleModel { get; set; }
    public List<PermissionResponse> Permissions { get; set; }
}

public class ManageRoleModel
{
    public Guid? Id { get; set; }
    public string DisplayName { get; set; }
    public List<Guid>? PermissionIds { get; set; }
}