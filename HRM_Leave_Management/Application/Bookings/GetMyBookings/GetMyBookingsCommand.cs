using Application.Abstractions.Messaging;
using Application.Bookings.GetBooking;

namespace Application.Bookings.GetMyBookings;

public record GetMyBookingsCommand : ICommand<List<BookingResponse>>;