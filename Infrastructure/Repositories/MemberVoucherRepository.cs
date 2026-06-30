using Domain.MemberVouchers;

namespace Infrastructure.Repositories;

internal sealed class MemberVoucherRepository : Repository<MemberVoucher, MemberVoucherId>, IMemberVoucherRepository
{
    public MemberVoucherRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}