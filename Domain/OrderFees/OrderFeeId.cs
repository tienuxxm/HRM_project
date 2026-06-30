namespace Domain.OrderFees;

public record OrderFeeId(Guid Value)
{
    public static OrderFeeId New => new OrderFeeId(Guid.NewGuid());
}