using Domain.MembershipClasses;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class MembershipClassRepository : Repository<MembershipClass, MembershipClassId>,
    IMembershipClassRepository
{
    public MembershipClassRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public bool HasData()
    {
        return DbContext.Set<MembershipClass>().Any(x => x.IsActive);
    }

    public Task<MembershipClass?> GetLowestMembershipClass(CancellationToken cancellationToken = default)
    {
        return DbContext.Set<MembershipClass>().OrderBy(m => m.Level).FirstOrDefaultAsync(cancellationToken);
    }
}