using Application.Abstractions.Messaging;

namespace Application.Orders.RemoveLineItem;
public record RemoveLineItemCommand(Guid OrderId, Guid LineId) : ICommand;

