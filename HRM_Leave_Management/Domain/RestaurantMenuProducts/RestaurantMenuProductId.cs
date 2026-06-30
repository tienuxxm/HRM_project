namespace Domain.RestaurantMenuProducts;

public record RestaurantMenuProductId(Guid Value)
{
    public static RestaurantMenuProductId New => new RestaurantMenuProductId(Guid.NewGuid());
}