using Application.Abstractions.Messaging;
using Application.Vouchers.GetAllPaged;
using Application.Vouchers.GetOne;
using Domain.Abstractions;
using Domain.Vouchers;

namespace Application.Vouchers.GetAllValid;

public record GetAllValidPagedVoucherCommand(bool IncludePartner = false) : PagedQuery<Voucher, VoucherId>, ICommand<GetAllVoucherPagedResposne>;