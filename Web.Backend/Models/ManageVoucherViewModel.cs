using Application.MembershipClasses.GetOne;
using Application.Partners.GetOne;
using Application.Restaurants.GetAll;
using Domain.Extension;

namespace Web.Backend.Models;

public class ManageVoucherViewModel
{
    public Guid? PartnerId { get; set; }
    public string? PartnerName { get; set; }
    public List<PartnerResponse> PartnerResponses { get; set; }
    public ManageVoucherModel ManageVoucherModel { get; set; }
    public List<RestaurantResponse> RestaurantResponses { get; set; }
    public List<MembershipClassResponse> MembershipClassResponses { get; set; }
}

public class ManageVoucherModel
{
    public Guid? Id { get; set; }

    public string? PageTitle { get; set; }
    public string Title { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public string? Place { get; set; }
    public int Point { get; set; }
    public IFormFile VoucherImageUrl { get; set; }
    public string VoucherImage { get; set; }
    public Guid? PartnerId { get; set; }
    public string? QrCodeImage { get; set; }
    public string? Content { get; set; }
    public string? Conditions { get; set; }
    public string? PartnerName { get; set; }
    public int? LimitQuantity { get; set; }
    public int? DiscountValue { get; set; }
    public int? MinOrderValue { get; set; }
    public int? MaxDiscountValue { get; set; }
    public int? Index { get; set; }
    public bool? IsDefault { get; set; }
    public bool? IsUserVoucher { get; set; }
    public int? VoucherDefaultType { get; set; }
    public double? DiscountPercent { get; set; }
    public string? MemberIdsString { get; set; }
    public string? MemberClassIdsString { get; set; }
    public string[]? Restaurants => !string.IsNullOrEmpty(Place) ? Place?.Split(',') : Array.Empty<string>();

    public string[]? MemberIds =>
        !string.IsNullOrEmpty(MemberIdsString) ? MemberIdsString?.Split(',') : Array.Empty<string>();

    public string[]? MemberClassIds => !string.IsNullOrEmpty(MemberClassIdsString)
        ? MemberClassIdsString?.Split(',')
        : Array.Empty<string>();

    public DateTime StartDateUtc => StartDate != "null" ? StartDate.StringToDateTimeUtc() : DateTime.UtcNow;
    public DateTime EndDateUtc => EndDate != "null" ? EndDate.StringToDateTimeUtc() : DateTime.UtcNow;
    public DateTime? StartDateDefault { get; set; }
    public DateTime? EndedDateDefault { get; set; }
}