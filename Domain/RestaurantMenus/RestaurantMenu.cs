using Domain.Abstractions;
using Domain.RestaurantMenuProducts;
using Domain.Restaurants;

namespace Domain.RestaurantMenus;

public class RestaurantMenu : Entity<RestaurantMenuId>
{
    private RestaurantMenu()
    {
    }

    private RestaurantMenu(RestaurantMenuId id, RestaurantId restaurantId, string name) : base(id)
    {
        Name = name;
        RestaurantId = restaurantId;
        IsActive = true;
    }

    public static RestaurantMenu Create(RestaurantId restaurantId, string name)
    {
        return new RestaurantMenu(RestaurantMenuId.New, restaurantId, name);
    }

    public void Deactive()
    {
        IsActive = false;
    }

    public string Name { get; private set; }
    public RestaurantId RestaurantId { get; private set; }
    public Restaurant Restaurant { get; private set; }
    public bool IsActive { get; private set; }
    public List<RestaurantMenuProduct> MenuProducts { get; private set; }

    public void SetMenu(List<RestaurantMenuProduct> menuProducts)
    {
        MenuProducts = menuProducts;
    }
}