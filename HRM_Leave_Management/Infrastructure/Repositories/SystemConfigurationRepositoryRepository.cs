using Domain.SystemConfigurations;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class SystemConfigurationRepositoryRepository : Repository<SystemConfiguration, SystemConfigurationId>,
    ISystemConfigurationRepository
{
    public SystemConfigurationRepositoryRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<SystemConfiguration?> GetConfigByName(ConfigName configName,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<SystemConfiguration>()
            .FirstOrDefaultAsync(x => x.ConfigName.Equals(configName), cancellationToken);
    }
}