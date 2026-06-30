using Domain.Abstractions;

namespace Domain.Orders.Events;

public sealed record CancelOrderEvent(OrderId OrderId) : IDomainEvent;