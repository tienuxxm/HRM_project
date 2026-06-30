using Application.Abstractions.Messaging;

namespace Application.Bookings.CompleteBooking;

public record CompleteBookingCommand(Guid BookingId) : ICommand;