using Application.Roles.GetOne;

namespace Web.Backend.Models;

public class ManageUserViewModel
{
    public ManageUserModel ManageUserModel { get; set; }
    public List<RoleResponse> Roles { get; set; }
}

public class ManageUserModel
{
    public Guid? Id { get; set; }
    public string Name { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public List<Guid> RoleIds { get; set; }
}