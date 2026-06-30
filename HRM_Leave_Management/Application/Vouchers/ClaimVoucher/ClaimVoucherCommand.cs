using Application.Abstractions.Messaging;
using Domain.Vouchers;

namespace Application.Vouchers.ClaimVoucher;

public record ClaimVoucherCommand(VoucherId VoucherId) : ICommand<bool>;