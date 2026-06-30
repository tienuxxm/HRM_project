using Application.Abstractions.Messaging;
using Application.Restaurants.GetAll;

namespace Application.Restaurants.GetOne;

public record GetRestaurantDetailCommand(Guid RestaurantId) : ICommand<RestaurantResponse>;