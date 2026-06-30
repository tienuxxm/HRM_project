using Domain.PromotionToRestaurants;

namespace Infrastructure.Repositories
{
    internal sealed class PromotionToRestaurantRepository : Repository<PromotionToRestaurant, PromotionToRestaurantId>, IPromotionToRestaurantRepository
    {
        public PromotionToRestaurantRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
        }


        public Task<PromotionToRestaurant?> GetByIdAsync(PromotionToRestaurantId id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<PromotionToRestaurant> GetProductsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<PromotionToRestaurant>?> GetByIdsAsync(List<PromotionToRestaurant> ids, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}