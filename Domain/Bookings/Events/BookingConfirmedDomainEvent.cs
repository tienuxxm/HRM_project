using Domain.Abstractions;

namespace Domain.Bookings.Events;

public sealed record BookingConfirmedDomainEvent(BookingId BookingId) : IDomainEvent;