using Application.Abstractions.Messaging;
using Domain.Vouchers;

namespace Application.Vouchers.Delete;

public record DeleteVoucherCommand(VoucherId VoucherId) : ICommand;
