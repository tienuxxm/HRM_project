namespace Domain.Promotions;

public record PromotionId(Guid Value)
{
    public static PromotionId New() => new(Guid.NewGuid());
}