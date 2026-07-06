using Domain.Abstractions;

namespace Infrastructure.Authentication.Models;

public class AuthenticationErrors
{
    public static Error EmailExisted => new Error("Email.Existed", "Email đã tồn tại trên hệ thống");
    public static Error ServerError => new Error("Server.Error", "Có lỗi xảy ra");
    public static Error ChangePasswordError => new Error("Password.Error", "Thay đổi mật khẩu không thành công");
    public static Error UsernameExisted => new Error("Username.Existed", "Tên đăng nhập đã tồn tại trên hệ thống");
    public static Error DeleteUserError => new Error("DeleteUser.Error", "Xóa tài khoản không thành công");
    public static Error UserAlreadyExists => new Error("User.AlreadyExists", "Tài khoản đã tồn tại trên hệ thống");
}