using Domain.ProductOfRestaurants;

namespace Infrastructure.Repositories;

internal sealed class ProductOfRestaurantRepository : Repository<ProductOfRestaurant, ProductRestaurantId>,
    IProductOfRestaurantRepository
{
    public ProductOfRestaurantRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}