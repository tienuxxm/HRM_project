using Application.Abstractions.Messaging;

namespace Application.Bookings.ConfirmBooking;

public sealed record ConfirmBookingCommand(Guid BookingId) : ICommand;