using Application.Extensions;
using Domain.Vouchers;

namespace Application.Vouchers.GetOne;

public sealed class VoucherResponse
{
    public Guid Id { get; init; }

    public string TitleVoucher { get; init; }

    public string? Place { get; init; }
    public bool? IsUserVoucher { get; set; }
    public string? Members { get; set; }
    public string? Memberships { get; set; }
    public int Point { get; init; }
    public string? QrCodeId { get; init; }
    public string? MemberCode { get; set; }

    public DateTime CreatedDate { get; init; }

    public DateTime? StartedDate { get; init; }

    public DateTime? EndedDate { get; init; }
    public string Date => StartedDate?.ToString("dd/MM/yyyy") + " - " + EndedDate?.ToString("dd/MM/yyyy");

    public string GetAvalialbeDate
    {
        get
        {
            if (StartedDate.HasValue && EndedDate.HasValue)
                return StartedDate.Value.ToLocalTime().ToString("dd/MM/yyyy");
            if (StartedDate.HasValue && !EndedDate.HasValue)
                return StartedDate.Value.ToLocalTime().ToString("dd/MM/yyyy");
            if (EndedDate.HasValue && !StartedDate.HasValue)
                return EndedDate.Value.ToLocalTime().ToString("dd/MM/yyyy");
            return "-";
        }
    }

    public bool? IsVoucherDefault { get; init; }

    public VoucherStatus Status { get; init; }
    public string StatusDisplay => Status.GetDescription();

    public string? ContentVoucher { get; init; }

    public string? ImageUrl { get; set; }
    public string? ImageId { get; set; }

    public Guid? PartnerId { get; set; }
    public string? PartnerName { get; set; }
    public string? Conditions { get; set; }
    public string? QrCodeImageUrl { get; set; }
    public string? QrCodeImageId { get; set; }
    public string? QrCode { get; set; }
    public int? LimitQuantity { get; set; }
    public int? DiscountValue { get; set; }
    public int? MaxDiscountValue { get; set; }
    public double? DiscountPercent { get; set; }
    public int? MinOrderValue { get; set; }
    public int? Index { get; set; }
}