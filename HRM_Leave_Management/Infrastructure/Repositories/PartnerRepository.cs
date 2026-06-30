using Domain.Partners;
using Domain.Vouchers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class PartnerRepository : Repository<Partner, PartnerId>, IPartnerRepository
{
    public PartnerRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public new async Task<Partner?> GetByIdAsync(
        PartnerId id,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Partner>()
            .Include(p => p.Vouchers)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task<List<Partner>?> Pagination(int take, int skip, string? search,
        CancellationToken cancellationToken = default)
    {
        var result = await DbContext.Set<Partner>().Where(p =>
                search == null || p.PartnerName.Value.ToLower().Contains(search.ToLower()))
            .Include(p => p.Vouchers)
            .Take(take)
            .Skip(skip)
            .ToListAsync(cancellationToken);

        return result;
    }
}