using System.Linq.Expressions;
using Domain.Abstractions;
using Domain.Restaurants;

namespace Domain.Bookings;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(BookingId id, CancellationToken cancellationToken = default);

    Task<bool> IsOverlappingAsync(
        Restaurant restaurant,
        BookingTime bookingTime,
        CancellationToken cancellationToken = default);

    void Add(Booking booking);
    void Update(Booking booking);

    Task<Booking?> GetLatestByProperty(Expression<Func<Booking, dynamic>> expression,
        CancellationToken cancellationToken = default);

    Task<PagedList<Booking>> GetAllPaged(PagedQuery<Booking, BookingId> request,
        IQueryable<Booking>? queryable = null);

    IQueryable<Booking> GetEntitiesAsQueryable();
}