using Domain.Districts;

namespace Infrastructure.Repositories;

internal sealed class DistrictRepository : Repository<District, DistrictId>, IDistrictRepository
{
    public DistrictRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}