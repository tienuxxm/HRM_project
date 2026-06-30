using Domain.RoleToPermissions;
using Domain.UserToRoles;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class RoleToPermissionRepository : Repository<RoleToPermission, RoleToPermissionId>, IRoleToPermissionRepository
{
    public RoleToPermissionRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}