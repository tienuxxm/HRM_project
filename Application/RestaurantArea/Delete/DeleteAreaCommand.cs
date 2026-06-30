using Application.Abstractions.Messaging;

namespace Application.RestaurantArea.Delete;

public record DeleteAreaCommand(Guid id) : ICommand;