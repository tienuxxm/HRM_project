using Application.Abstractions.Messaging;
using Application.Vouchers.GetOne;

namespace Application.Vouchers.GetAllValid;

public record GetAllValidVoucherCommand(bool IncludePartner = false) : ICommand<List<VoucherResponse>>;