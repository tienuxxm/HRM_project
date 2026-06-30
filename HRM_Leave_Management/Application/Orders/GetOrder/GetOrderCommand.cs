using Application.Abstractions.Messaging;

namespace Application.Orders.GetOrder;

public sealed record GetOrderCommand(Guid OrderId) : ICommand<OrderResponse>;