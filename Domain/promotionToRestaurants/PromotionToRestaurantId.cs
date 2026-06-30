namespace Domain.PromotionToRestaurants;

public record PromotionToRestaurantId(Guid Value)
{
    public static PromotionToRestaurantId New() => new(Guid.NewGuid());
}