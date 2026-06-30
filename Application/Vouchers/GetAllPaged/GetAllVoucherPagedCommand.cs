using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Vouchers;

namespace Application.Vouchers.GetAllPaged;

public record GetAllVoucherPagedCommand(Guid? PartnerId = null) : PagedQuery<Voucher, VoucherId>,
    ICommand<GetAllVoucherPagedResposne>;