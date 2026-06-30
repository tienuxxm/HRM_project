using Application.Abstractions.Messaging;

namespace Application.Bookings.GetBooking;

public record GetBookingCommand(Guid BookingId) : ICommand<BookingResponse>;