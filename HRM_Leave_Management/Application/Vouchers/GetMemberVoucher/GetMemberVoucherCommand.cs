using Application.Abstractions.Messaging;
using Application.Vouchers.GetOne;
using Domain.Members;
using Domain.Vouchers;

namespace Application.Vouchers.GetMemberVoucher;

public record GetMemberVoucherCommand(PhoneNumber PhoneNumber, VoucherId VoucherId) : ICommand<VoucherResponse>;