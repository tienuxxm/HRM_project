using Domain.Abstractions;

namespace Domain.Bookings.Events;

public sealed record BookingCancelledDomainEvent(BookingId BookingId) : IDomainEvent;