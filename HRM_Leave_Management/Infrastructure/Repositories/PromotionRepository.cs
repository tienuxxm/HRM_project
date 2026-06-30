using Domain.Abstractions;
using Domain.Members;
using Domain.Promotions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    internal sealed class PromotionRepository : Repository<Promotion, PromotionId>, IPromotionRepository
    {
        public PromotionRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task<Promotion?> GetByIdAsync(PromotionId id)
        {
            return await DbContext.Set<Promotion>()
                .FirstOrDefaultAsync(p => p.Id == id);
        }


        public async Task<List<Promotion>?> Search(string searchValue, CancellationToken cancellationToken)
        {
            var result = DbContext.Set<Promotion>()
                .Where(x => x.PromotionName.Value.Contains(searchValue))
                .ToList();
            return result;
        }
    }
}