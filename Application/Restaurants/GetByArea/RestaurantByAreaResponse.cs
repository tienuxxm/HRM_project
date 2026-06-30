using Application.Restaurants.GetAll;

namespace Application.Restaurants.GetByArea;

public class RestaurantByAreaResponse
{
    public string AreaName { get; set; }
    public List<RestaurantResponse> Restaurants { get; set; }
}