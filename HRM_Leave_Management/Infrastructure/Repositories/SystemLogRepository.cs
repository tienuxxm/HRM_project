using Domain.SystemLog;

namespace Infrastructure.Repositories;

internal sealed class SystemLogRepository : Repository<SystemLog, SystemLogId>, ISystemLogRepository
{
    public SystemLogRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}