using Domain.Abstractions;

namespace Application.Members.RegisterWithFacebook;

public class RegisterFacebookError
{
    public static Error EmailExisted => new("Email.Existed", "Email đã tồn tại trên hệ thống");
    public static Error AlreadyRegister => new("Account.AlreadyRegistered", "Account này đã được đăng kí");
    public static Error PhoneExisted => new("Phone.Existed", "SĐT đã tồn tại trên hệ thống");
}