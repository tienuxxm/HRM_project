using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Dapper;

namespace Application.Bookings.GetBooking;

internal sealed class GetBookingQueryHandler : IQueryHandler<GetBookingQuery, BookingResponse>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetBookingQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<BookingResponse>> Handle(GetBookingQuery request, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        const string sql = """
            SELECT
                id AS Id,
                apartment_id AS ApartmentId,
                user_id AS UserId,
                status AS Status,
                total_of_people AS TotalOfPeople
                created_date AS CreatedDate
            FROM bookings
            WHERE id = @BookingId
            """;

        var booking = await connection.QueryFirstOrDefaultAsync<BookingResponse>(
            sql,
            new
            {
                request.BookingId
            });

        return booking;
    }
}