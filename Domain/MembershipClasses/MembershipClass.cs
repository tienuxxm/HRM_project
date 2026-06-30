using Domain.Abstractions;
using Domain.MembershipBenefits;
using Domain.Shared;

namespace Domain.MembershipClasses;

public sealed class MembershipClass : Entity<MembershipClassId>
{
    private MembershipClass()
    {
    }

    private MembershipClass(MembershipClassId id, ClassName className, Level level, Money maxMoney,
        float percentDefault,
        float percentBirthDate, int effectiveYears) : base(id)
    {
        ClassName = className;
        Level = level;
        MaxMoney = maxMoney;
        IsActive = true;
        PercentDefault = percentDefault;
        PercentBirthDate = percentBirthDate;
        EffectiveYear = effectiveYears;
    }

    public ClassName ClassName { get; private set; }
    public Level Level { get; private set; }
    public float PercentDefault { get; private set; }
    public float PercentBirthDate { get; private set; }
    public Money MaxMoney { get; private set; }
    public bool IsActive { get; private set; }
    public int? EffectiveYear { get; private set; }
    public List<MembershipBenefit> MembershipBenefits { get; private set; }

    public void SetBenefits(List<MembershipBenefit> benefits)
    {
        MembershipBenefits = benefits;
    }

    public void Deactive()
    {
        IsActive = false;
    }

    public Result Update(ClassName className, Level level, Money maxMoney, float percentDefault, float percentBirthDate,
        int effetiveYears)
    {
        ClassName = className;
        MaxMoney = maxMoney;
        PercentDefault = percentDefault;
        PercentBirthDate = percentBirthDate;
        EffectiveYear = effetiveYears;
        return Result.Success();
    }

    public static MembershipClass Create(ClassName className, Level level, Money maxMoney, float percentDefault,
        float percentBirthDate, int effectiveYears)
    {
        return new MembershipClass(MembershipClassId.New, className, level, maxMoney, percentDefault, percentBirthDate,
            effectiveYears);
    }
}