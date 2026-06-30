using Domain.Restaurants;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class RestaurantsRepository : Repository<Restaurant, RestaurantId>, IRestaurantRepository
{
    public RestaurantsRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<List<Restaurant>?> GetAllWithOutDeleted(CancellationToken cancellationToken)
    {
        return await DbContext.Set<Restaurant>()
            .Include(x => x.RestaurantArea)
            .Where(x => !x.IsDeleted).ToListAsync(cancellationToken);
    }
}