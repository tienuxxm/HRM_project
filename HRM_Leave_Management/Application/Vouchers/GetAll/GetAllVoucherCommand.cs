using Application.Abstractions.Messaging;
using Application.Vouchers.GetOne;

namespace Application.Vouchers.GetAll;

public record GetAllVoucherCommand() : ICommand<List<VoucherResponse>>;