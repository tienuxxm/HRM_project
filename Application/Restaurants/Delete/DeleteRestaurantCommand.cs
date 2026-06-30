using Application.Abstractions.Messaging;
using Domain.Restaurants;

namespace Application.Restaurants.Delete;

public sealed record DeleteRestaurantCommand(RestaurantId RestaurantId) : ICommand;