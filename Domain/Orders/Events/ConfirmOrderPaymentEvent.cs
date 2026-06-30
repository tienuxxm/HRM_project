using Domain.Abstractions;

namespace Domain.Orders.Events;

public sealed record ConfirmOrderPaymentEvent(OrderId OrderId) : IDomainEvent;