using Application.Abstractions.Messaging;

namespace Application.Bookings.RejectBooking;

public sealed record RejectBookingCommand(Guid BookingId) : ICommand;