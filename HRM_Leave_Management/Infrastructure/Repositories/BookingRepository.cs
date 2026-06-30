using Domain.Restaurants;
using Domain.Bookings;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class BookingRepository : Repository<Booking, BookingId>, IBookingRepository
{
    private static readonly BookingStatus[] ActiveBookingStatuses =
    {
        BookingStatus.Reserved,
        BookingStatus.Confirmed,
        BookingStatus.Completed
    };

    public BookingRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<bool> IsOverlappingAsync(Restaurant restaurant, BookingTime bookingTime, CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Booking>()
            .AnyAsync(
                booking =>
                    booking.BookingTime == bookingTime.GetDateTime() &&
                    ActiveBookingStatuses.Contains(booking.Status),
                cancellationToken);
    }
}