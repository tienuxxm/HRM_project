using Domain.Abstractions;

namespace Domain.Members;

public class MemberErrors
{
    public static Error NotFound = new(
        "Member.Found",
        "Người dùng không tồn tại");

    public static Error InvalidCredentials = new(
        "Member.InvalidCredentials",
        "Account or mật khẩu không chính xác");

    public static Error PhoneNumberNotFound =
        new("Member.PhoneNumber.Found", "The member with specified phone number was not found");

    public static Error MemberExisted =
        new("Member.Existed", "Người dùng đã tồn tại");

    public static Error PhoneCodeValidateFail =
        new("Member.PhoneMumber.ValidateCode", "Fail to login member with this validation code");

    public static Error PhoneNumberExisted =
        new("Member.PhoneMumber.Existed", "Số điện thoại đã được sủ dụng");

    public static Error EmailExisted =
        new("Member.Email.Existed", "Email đã được sủ dụng");
}