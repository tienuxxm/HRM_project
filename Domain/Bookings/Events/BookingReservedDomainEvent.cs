using Domain.Abstractions;

namespace Domain.Bookings.Events;

public sealed record BookingReservedDomainEvent(BookingId BookingId) : IDomainEvent;