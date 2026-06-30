using Domain.Abstractions;

namespace Domain.Orders.Events;

public sealed record ProcessOrderEvent(OrderId OrderId) : IDomainEvent;