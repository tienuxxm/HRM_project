using Domain.Abstractions;

namespace Domain.MemberVouchers;

public interface IMemberVoucherRepository
{
    void Add(MemberVoucher memberVoucher);
    IQueryable<MemberVoucher> GetEntitiesAsQueryable();

    public void UpdateRange(List<MemberVoucher> entities);

    Task<PagedList<MemberVoucher>> GetAllPaged(PagedQuery<MemberVoucher, MemberVoucherId> request,
        IQueryable<MemberVoucher>? queryable = null);
}