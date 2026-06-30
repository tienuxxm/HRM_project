using Domain.Provinces;

namespace Infrastructure.Repositories;

internal sealed class ProvinceRepository : Repository<Province, ProvinceId>, IProvinceRepository
{
    public ProvinceRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}