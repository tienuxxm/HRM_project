using Application.Abstractions.Messaging;

namespace Application.RestaurantArea.Create;

public record CreateAreaCommand(string AreaName) : ICommand<Guid>;