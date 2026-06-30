using Domain.Shared;

namespace Application.Members.Responses;

public sealed class MemberResponse
{
    public Guid Id { get; init; }

    public string Email { get; init; }

    public string FirstName { get; init; }

    public string LastName { get; set; }
    public string FullName => FirstName + " " + LastName;
    public string MemberCode { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public string? AvatarUrl { get; set; }
    public string? MembershipClass { get; set; }
    public decimal? MoneyForNextClass { get; set; }
    public string? Currency { get; set; }
    public int? MemberPoint { get; set; }
    public int? TotalValidVoucher { get; set; }
    public Money? TotalPaid { get; set; }
    public string TotalPaidDisplay => TotalPaid != null ? (TotalPaid.Amount + " " + TotalPaid.Currency.Code) : "0 VND";
    public DateTime? BirthDate { get; set; }
    public int? DistrictId { get; set; }
    public int? ProvinceId { get; set; }
    public Guid? QrCodeId { get; set; }
    public string? DiscountValue { get; set; }
    public DateTime? MembershipAssignedDate { get; set; }
    public DateTime? MembershipExpiredDate => MembershipAssignedDate?.AddYears(1) ?? null;
    public string? Note { get; set; }
}