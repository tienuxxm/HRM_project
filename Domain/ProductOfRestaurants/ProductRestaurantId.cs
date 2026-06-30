namespace Domain.ProductOfRestaurants;

public record ProductRestaurantId(Guid Value)
{
    public static ProductRestaurantId New => new ProductRestaurantId(Guid.NewGuid());
}