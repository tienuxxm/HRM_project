using Domain.Permissions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class PermissionRepository : Repository<Permission, PermissionId>, IPermissionRepository
{
    public PermissionRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
    
    public new async Task<Permission?> GetByIdAsync(
        PermissionId id,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Permission>()
            .Include(p => p.Roles)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task<List<Permission>?> Pagination(int take, int skip, string? search, CancellationToken cancellationToken = default)
    {
        var result = await DbContext.Set<Permission>()
            .Where(r => search != null ? r.DisplayName.Value.ToLower().Contains(search): true)
            .Include(r => r.Roles)
            .Take(take)
            .Skip(skip)
            .ToListAsync(cancellationToken);
        return result;
    }
    
}