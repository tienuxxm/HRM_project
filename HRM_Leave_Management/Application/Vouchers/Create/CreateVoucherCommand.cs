using Application.Abstractions.Messaging;
using Domain.Vouchers;

namespace Application.Vouchers.Create;

public sealed record CreateVoucherCommand(
    string TitleVoucher,
    string ImageUrl,
    DateTime StartedDate,
    DateTime EndedDate,
    string? Place,
    int Point,
    Guid? PartnerId,
    string? ContentVoucher,
    string? Conditions,
    int? LimitQuantity,
    int? DiscountValue,
    double? DiscountPercent,
    int? MinOrderValue,
    int? MaxDiscountValue,
    int? Index,
    bool? IsDefault,
    VoucherDefaultType? VoucherDefaultType,
    string[]? MemberIds,
    string[]? MembershipIds
) : ICommand<Guid>;