using Application.Abstractions.Messaging;
using Domain.Partners;
using Domain.Shared;
using Domain.Vouchers;

namespace Application.Vouchers.Update;

public sealed record UpdateVoucherCommand(
    VoucherId VoucherId,
    TitleVoucher? TitleVoucher,
    ImageUrl? ImageUrl,
    DateTime? StartedDate,
    DateTime? EndedDate,
    Place? Place,
    int? Point,
    PartnerId? PartnerId,
    ContentVoucher? ContentVoucher,
    Conditions? Conditions,
    int? LimitQuantity,
    int? DiscountValue,
    double? DiscountPercent,
    int? MinOrderValue,
    int? MaxDiscountValue,
    int? Index,
    VoucherDefaultType? VoucherDefaultType
) : ICommand<Voucher>;