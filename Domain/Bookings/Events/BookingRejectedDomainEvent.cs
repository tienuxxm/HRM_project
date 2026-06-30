using Domain.Abstractions;

namespace Domain.Bookings.Events;

public sealed record BookingRejectedDomainEvent(BookingId BookingId) : IDomainEvent;