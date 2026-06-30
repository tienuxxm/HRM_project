using System.Globalization;
using Domain.Shared;

namespace Application.MembershipClasses.GetOne;

public class MembershipClassResponse
{
    public Guid Id { get; set; }
    public string ClassName { get; set; }
    public int Level { get; set; }
    public Money MaxMoney { get; set; }
    public float PercentDefault { get; set; }
    public float PercentBirthDate { get; set; }
    public bool IsActive { get; set; }
    public int EffectiveYears { get; set; }

    public string MaxMoneyDisplay =>
        MaxMoney.Amount.ToString("#,###", CultureInfo.GetCultureInfo("vi-VN").NumberFormat) +
        " " + MaxMoney.Currency.Code;

    public string BenefitsDisplay => string.Join(',', MembershipBenefits.Select(x => x.Title).ToList());
    public List<MembershipBenefitResponse> MembershipBenefits { get; set; }
}

public class MembershipBenefitResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}