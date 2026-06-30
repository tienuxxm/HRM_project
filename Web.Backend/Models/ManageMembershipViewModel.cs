using Application.MembershipClasses.GetOne;

namespace Web.Backend.Models;

public class ManageMembershipViewModel
{
    public Guid? Id { get; set; }
    public string ClassName { get; set; }
    public int Level { get; set; }
    public decimal MaxMoney { get; set; }
    public float PercentDefault { get; set; }

    public float PercentBirthDate { get; set; }
    public int EffectiveYears { get; set; }
    public List<MembershipBenefitResponse> Benefits { get; set; }
}