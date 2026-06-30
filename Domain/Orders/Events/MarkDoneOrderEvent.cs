using Domain.Abstractions;

namespace Domain.Orders.Events;

public sealed record MarkDoneOrderEvent(OrderId OrderId) : IDomainEvent;