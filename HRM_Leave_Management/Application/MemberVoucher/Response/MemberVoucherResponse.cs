using Domain.MemberVouchers;

namespace Application.MemberVoucher.Response;

public class MemberVoucherResponse
{
    public string TitleVoucher { get; set; }
    public DateTime EndedDate { get; set; }
    public DateTime StartedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public MemberVoucherStatus Status { get; set; }

    public string GetAvalialbeDate => StartedDate.ToLocalTime().ToString("dd/MM/yyyy") + " - " +
                                      EndedDate.ToLocalTime().ToString("dd/MM/yyyy");

    public Guid Id { get; init; }
    public string? Place { get; init; }
    public int Point { get; init; }
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
}