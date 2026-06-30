using Domain.Abstractions;
using Domain.Shared;

namespace Domain.FreeServices;

public class FeeService : Entity<FeeServiceId>
{
    private FeeService()
    {
    }

    private FeeService(FeeServiceId id, FeeName feeName, bool isPercent, Money? feeAmount, float? feePercent,
        FeeType feeType, bool isActive) : base(id)
    {
        FeeName = feeName;
        FeeAmount = feeAmount;
        IsPercent = isPercent;
        FeePercent = feePercent;
        FeeType = feeType;
        IsActive = isActive;
    }

    public FeeType FeeType { get; private set; }

    public FeeName FeeName { get; private set; }
    public Money? FeeAmount { get; private set; }
    public float? FeePercent { get; private set; }
    public bool IsPercent { get; private set; }
    public bool IsActive { get; private set; }

    public string GetInvoiceFeeString
    {
        get
        {
            if (IsPercent)
                return FeePercent + "%";
            return FeeAmount?.Amount + FeeAmount?.Currency.Code;
        }
    }

    public static FeeService Create(FeeName feeName, bool isPercent, Money? feeAmount, float? feePercent,
        FeeType feeType, bool isActive)
    {
        return new FeeService(FeeServiceId.New, feeName, isPercent, feeAmount, feePercent, feeType, isActive);
    }

    public void Update(
        FeeName feeName,
        bool isPercent,
        Money? feeAmount,
        float? feePercent,
        FeeType feeType,
        bool isActive)
    {
        FeeName = feeName;
        IsPercent = isPercent;
        FeeAmount = feeAmount;
        FeePercent = feePercent;
        FeeType = feeType;
        IsActive = isActive;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}