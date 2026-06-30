using Domain.Abstractions;
using Domain.Categories;

namespace Domain.Partners;

public interface IPartnerRepository
{
    Task<Partner?> GetByIdAsync(PartnerId id, CancellationToken cancellationToken = default);

    void Add(Partner partner);

    void Update(Partner partner);

    void Remove(Partner partner);

    IQueryable<Partner> GetEntitiesAsQueryable();

    Task<List<Partner>?> GetAll(CancellationToken cancellationToken = default);
    Task<List<Partner>?> Pagination(int take, int skip, string? search, CancellationToken cancellationToken = default);

    public Task<PagedList<Partner>> GetAllPaged(PagedQuery<Partner, PartnerId> request,
        IQueryable<Partner>? queryable = null);
}