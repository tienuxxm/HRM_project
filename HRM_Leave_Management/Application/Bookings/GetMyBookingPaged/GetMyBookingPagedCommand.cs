using Application.Abstractions.Messaging;
using Application.Bookings.GetBooking;
using Domain.Abstractions;
using Domain.Bookings;

namespace Application.Bookings.GetMyBookingPaged;

public record GetMyBookingPagedCommand : PagedQuery<Booking, BookingId>, ICommand<PagedList<BookingResponse>>;