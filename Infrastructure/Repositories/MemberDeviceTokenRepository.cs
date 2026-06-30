using Domain.MemberDeviceTokens;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class MemberDeviceTokenRepository : Repository<MemberDeviceToken, MemberDeviceTokenId>,
    IMemberDeviceTokenRepository
{
    public MemberDeviceTokenRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<MemberDeviceToken?> GetByIdentityId(string identityId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<MemberDeviceToken>()
            .FirstOrDefaultAsync(x => x.IdentityId == identityId, cancellationToken);
    }

    public async Task<bool> IsExisted(string token, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<MemberDeviceToken>().AnyAsync(x => x.DeviceToken == token, cancellationToken);
    }

    public async Task<List<MemberDeviceToken>> GetDeviceTokenAsync(List<string> identityIds,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<MemberDeviceToken>().Where(x => identityIds.Any(id => id == x.IdentityId))
            .Distinct().ToListAsync(cancellationToken);
    }
}