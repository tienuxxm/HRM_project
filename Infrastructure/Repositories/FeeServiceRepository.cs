using Domain.FreeServices;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class FeeServiceRepository : Repository<FeeService, FeeServiceId>, IFeeServiceRepository
{
    public FeeServiceRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<List<FeeService>?> GetAllActive(CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<FeeService>()
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);
    }

    public bool HasData()
    {
        return DbContext.Set<FeeService>().Any(x => x.IsActive);
    }
}