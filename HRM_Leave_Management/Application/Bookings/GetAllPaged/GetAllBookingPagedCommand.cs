using Application.Abstractions.Messaging;
using Application.Bookings.GetBooking;
using Domain.Abstractions;
using Domain.Bookings;

namespace Application.Bookings.GetAllPaged;

public record GetAllBookingPagedCommand(
    Guid? MemberId
    ) : PagedQuery<Booking, BookingId>, ICommand<PagedList<BookingResponse>>;