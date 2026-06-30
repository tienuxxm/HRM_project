namespace Domain.RestaurantMenus;

public record RestaurantMenuId(Guid Value)
{
    public static RestaurantMenuId New => new RestaurantMenuId(Guid.NewGuid());
}