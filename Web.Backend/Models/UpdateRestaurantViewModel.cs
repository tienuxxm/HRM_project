using Application.Extensions;
using Domain.Restaurants;

namespace Web.Backend.Models;

public class UpdateRestaurantViewModel
{
    public UpdateRestaurantModel UpdateRestaurantModel { get; set; }
}

public class UpdateRestaurantModel : ManageRestaurantModel
{
    public Guid Id { get; set; }
}