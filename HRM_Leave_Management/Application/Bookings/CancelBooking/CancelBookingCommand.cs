using Application.Abstractions.Messaging;

namespace Application.Bookings.CancelBooking;

public record CancelBookingCommand(Guid BookingId) : ICommand;