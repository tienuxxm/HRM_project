namespace Domain.RestaurantAreas;

public record RestaurantAreaId(Guid Value)
{
    public static RestaurantAreaId New => new RestaurantAreaId(Guid.NewGuid());
}