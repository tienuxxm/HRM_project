using Application.Abstractions.Messaging;

namespace Application.Restaurants.Search;

public sealed record SearchRestaurantsQuery(
    DateOnly BookingDate,
    TimeOnly StartTime,
    TimeOnly EndTime) : IQuery<IReadOnlyList<RestaurantResponse>>;