using Domain.Abstractions;

namespace Domain.Orders.Events;

public sealed record CreateOrderEvent(OrderId OrderId) : IDomainEvent;