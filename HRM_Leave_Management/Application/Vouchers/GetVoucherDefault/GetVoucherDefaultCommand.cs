using Application.Abstractions.Messaging;
using Application.Vouchers.GetOne;
using Domain.Vouchers;

namespace Application.Vouchers.GetVoucherDefault;

public record GetVoucherDefaultCommand(VoucherDefaultType VoucherDefaultType) : ICommand<VoucherResponse>;