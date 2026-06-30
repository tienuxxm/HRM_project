using Application.Roles.GetOne;
using Application.Users.GetOne;

namespace Web.Backend.Models
{
    public class ModalUserModel
    { 
        public UserResponse? User { get; set; }
        public List<RoleResponse>? Roles { get; set; }
    }
}