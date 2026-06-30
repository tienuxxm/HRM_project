using Domain.Abstractions;

namespace Domain.Bookings.Events;

public sealed record BookingCompletedDomainEvent(BookingId BookingId) : IDomainEvent;