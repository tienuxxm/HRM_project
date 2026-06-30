using Application.Abstractions.Messaging;
using Application.MemberVoucher.Response;

namespace Application.Members.GetMemberVoucherByPhone;

public record MemberVoucherByPhoneCommand(string PhoneNumber) : ICommand<List<MemberVoucherResponse>>;