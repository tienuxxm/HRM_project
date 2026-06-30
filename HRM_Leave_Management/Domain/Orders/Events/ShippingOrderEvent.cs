using Domain.Abstractions;

namespace Domain.Orders.Events;

public sealed record ShippingOrderEvent(OrderId OrderId) : IDomainEvent;