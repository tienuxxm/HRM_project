using Domain.Abstractions;
using Domain.Restaurants;

namespace Domain.RestaurantAreas;

public class RestaurantArea : Entity<RestaurantAreaId>
{
    public AreaName AreaName { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    public List<Restaurant> Restaurants { get; private set; }

    private RestaurantArea()
    {
    }

    private RestaurantArea(RestaurantAreaId id, AreaName areaName, DateTime createdAt) : base(id)
    {
        AreaName = areaName;
        CreatedAt = createdAt;
        IsActive = true;
    }

    public static RestaurantArea Create(AreaName areaName, DateTime createdAt)
    {
        return new RestaurantArea(RestaurantAreaId.New, areaName, createdAt);
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}