using Domain.Roles;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class RoleRepository : Repository<Role, RoleId>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
    
    public new async Task<Role?> GetByIdAsync(
        RoleId id,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Role>()
            .Include(u => u.Users)
            .ThenInclude(utr => utr.User)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }
    
    public async Task<List<Role>?> Pagination(int take, int skip, string? search, CancellationToken cancellationToken = default)
    {
        var result = await DbContext.Set<Role>()
            .Where(r => search != null ? r.DisplayName.Value.ToLower().Contains(search): true)
            .Include(r => r.Users).ThenInclude(utr => utr.User)
            .Take(take)
            .Skip(skip)
            .ToListAsync(cancellationToken);
        return result;
    }
    
}