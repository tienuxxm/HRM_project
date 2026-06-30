using Domain.Positions;

namespace Infrastructure.Repositories
{
    internal sealed class PositionRepository : Repository<Position, PositionId>, IPositionRepository
    {
        public PositionRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
        }
    }
}
