using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class UserRepository : Repository<User, UserId>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public new async Task<User?> GetByIdAsync(
        UserId id,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<User>()
            .Include(u => u.Roles)
            .ThenInclude(utr => utr.Role)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }


    public async Task<List<User>?> Pagination(int take, int skip, CancellationToken cancellationToken = default)
    {
        var result = await DbContext.Set<User>()
            .Include(u => u.Roles)
            .ThenInclude(utr => utr.Role)
            .Take(take)
            .Skip(skip)
            .ToListAsync(cancellationToken);
        return result;
    }

    public async Task<User?> FindUniqEmail(UserId userId, Domain.Users.Email email,
        CancellationToken cancellationToken = default)
    {
        var result = await DbContext.Set<User>().FirstOrDefaultAsync(v => (
            !v.Id.Equals(userId)
            && v.Email == email
            && !(v.IsDeleted.HasValue && v.IsDeleted.Value)), cancellationToken);

        return result;
    }
}
