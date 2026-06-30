using Application.Abstractions.Messaging;

namespace Application.Orders.AddLineItem;

public sealed record  AddLineItemCommand(
    Guid OrderId,
    Guid ProductId,
    int Amount) : ICommand;


