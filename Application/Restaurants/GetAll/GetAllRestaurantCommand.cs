using Application.Abstractions.Messaging;

namespace Application.Restaurants.GetAll;

public class GetAllRestaurantCommand : ICommand<List<RestaurantResponse>>
{
}