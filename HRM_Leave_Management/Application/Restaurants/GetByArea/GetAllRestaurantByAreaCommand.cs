using Application.Abstractions.Messaging;

namespace Application.Restaurants.GetByArea;

public record GetAllRestaurantByAreaCommand() : ICommand<List<RestaurantByAreaResponse>>;