using Application.Vouchers.GetOne;

namespace Application.Partners.GetOne;

public sealed class PartnerResponse
{
    public Guid Id { get; init; }
    public string PartnerName { get; init; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? ImageUrl { get; set; }
    public string? QrCodeId { get; set; }
    public DateTime CreatedDate { get; init; }

    public List<VoucherResponse>? Vouchers { get; set; }
}