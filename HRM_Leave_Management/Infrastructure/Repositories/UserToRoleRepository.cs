using Domain.UserToRoles;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class UserToRoleRepository : Repository<UserToRole, UserToRoleId>, IUserToRoleRepository
{
    public UserToRoleRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}