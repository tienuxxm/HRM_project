using Domain.Vouchers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class VoucherRepository : Repository<Voucher, VoucherId>, IVoucherRepository
{
    public VoucherRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public new async Task<Voucher?> GetByIdAsync(
        VoucherId id,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Voucher>()
            .Include(x => x.Partner)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task<List<Voucher>?> Pagination(int take, int skip, string? search,
        CancellationToken cancellationToken = default)
    {
        var result = await DbContext.Set<Voucher>()
            .Where(v =>
                search == null || (v.TitleVoucher.Value.ToLower().Contains(search.ToLower())
                                   || v.TitleVoucher.Value.ToLower().Contains(search.ToLower())))
            .Take(take).Skip(skip)
            .ToListAsync(cancellationToken);
        return result;
    }
}