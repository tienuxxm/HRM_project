namespace Domain.Restaurants;

public record RestaurantId(Guid Value)
{
    public static RestaurantId New() => new(Guid.NewGuid());
}