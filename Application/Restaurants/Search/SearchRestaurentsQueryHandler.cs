using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Dapper;

namespace Application.Restaurants.Search;

public class SearchRestaurentsQueryHandler  : IQueryHandler<SearchRestaurantsQuery, IReadOnlyList<RestaurantResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public SearchRestaurentsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }
    
    public async Task<Result<IReadOnlyList<RestaurantResponse>>> Handle(SearchRestaurantsQuery request, CancellationToken cancellationToken)
    {
        if (request.StartTime > request.EndTime)
        {
            return new List<RestaurantResponse>();
        }
        
        using var connection = _sqlConnectionFactory.CreateConnection();
        var dayOfWeek = request.BookingDate.DayOfWeek;
        const string sql = """
                           SELECT
                               r.id AS Id,
                               r.name AS Name,
                               r.description AS Description,
                               r.address_country AS Country,
                               r.address_state AS State,
                               r.address_zip_code AS ZipCode,
                               r.address_city AS City,
                               r.address_street AS Street
                           FROM restaurants AS r
                           WHERE EXISTS
                           (
                               SELECT 1
                               FROM workdays AS w
                               WHERE
                                   b.restaurant_id = r.id AND
                                   b.day_of_week = @DayOfWeek
                                   b.begin_shift <= @StartTime AND
                                   b.end_shift >= @EndTime AND
                                   b.is_active = 1
                           )
                           """;

        var apartments = await connection
            .QueryAsync<RestaurantResponse, AddressResponse, RestaurantResponse>(
                sql,
                (restaurant, address) =>
                {
                    restaurant.Address = address;
                    return restaurant;
                },
                new
                {
                    request.BookingDate.DayOfWeek,
                    request.StartTime,
                    request.EndTime
                });

        return apartments.ToList();
    }
}