using Application.Abstractions.Messaging;
using Domain.Vouchers;

namespace Application.Vouchers.GetOne;

public record GetOneVoucherCommand(VoucherId VoucherId) :  ICommand<VoucherResponse>;
