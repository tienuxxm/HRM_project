using Domain.RestaurantAreas;

namespace Infrastructure.Repositories;

internal sealed class RestaurantAreaRepository : Repository<RestaurantArea, RestaurantAreaId>, IRestaurantAreaRepository
{
    public RestaurantAreaRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}