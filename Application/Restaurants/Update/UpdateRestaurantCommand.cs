using Application.Abstractions.Messaging;
using Domain.RestaurantAreas;
using Domain.Restaurants;
using Domain.Shared;

namespace Application.Restaurants.Update;

public record UpdateRestaurantCommand(
    RestaurantId Id,
    RestaurantName? RestaurantName,
    Address? Address,
    TimeOnly? OpeningAt,
    TimeOnly? ClosingAt,
    RestaurantAreaId? RestaurantAreaId,
    ImageUrl? ImageUrl,
    string? mapLink
) : ICommand<Guid>;