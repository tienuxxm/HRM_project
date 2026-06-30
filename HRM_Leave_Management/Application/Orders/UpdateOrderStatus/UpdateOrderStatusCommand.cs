using Application.Abstractions.Messaging;
using Domain.Orders;

namespace Application.Orders.UpdateOrderStatus;

public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus OrderStatus) : ICommand;