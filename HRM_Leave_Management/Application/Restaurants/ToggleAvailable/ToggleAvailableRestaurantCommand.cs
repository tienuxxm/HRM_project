using Application.Abstractions.Messaging;
using Domain.Restaurants;

namespace Application.Restaurants.ToggleAvailable;

public record ToggleAvailableRestaurantCommand(RestaurantId RestaurantId, bool Toggle) : ICommand;