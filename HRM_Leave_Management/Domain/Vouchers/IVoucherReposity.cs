using Domain.Abstractions;
using Domain.Categories;

namespace Domain.Vouchers;

public interface IVoucherRepository
{
    Task<Voucher?> GetByIdAsync(VoucherId id, CancellationToken cancellationToken = default);

    void Add(Voucher voucher);

    void Update(Voucher voucher);

    void Remove(Voucher voucher);

    IQueryable<Voucher> GetEntitiesAsQueryable();

    Task<List<Voucher>?> GetByIdsAsync(List<VoucherId> ids, CancellationToken cancellationToken = default);
    Task<List<Voucher>?> GetAll(CancellationToken cancellationToken = default);

    Task<List<Voucher>?> Pagination(int take, int skip, string? search, CancellationToken cancellationToken = default);

    public Task<PagedList<Voucher>> GetAllPaged(PagedQuery<Voucher, VoucherId> request,
        IQueryable<Voucher>? queryable = null);
}