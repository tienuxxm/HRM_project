using Application.Abstractions.Messaging;

namespace Application.RestaurantArea.GetAll;

public record GetAllRestaurantAreaCommand() : ICommand<List<RestaurantAreaResponse>>;