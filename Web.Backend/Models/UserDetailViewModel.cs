using Application.Roles.GetOne;
using Application.Users.GetOne;

namespace Web.Backend.Models;

public class UserDetailViewModel
{
    public UserResponse UserDetail { get; set; }
    public List<RoleResponse> Roles { get; set; }
    public UserUpdateModel UserUpdateModel { get; set; }
}

public class UserUpdateModel
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}