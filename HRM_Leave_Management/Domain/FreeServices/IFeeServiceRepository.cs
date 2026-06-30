using Domain.Abstractions;

namespace Domain.FreeServices;

public interface IFeeServiceRepository
{
    Task<List<FeeService>?> GetAllActive(
        CancellationToken cancellationToken = default);

    void AddRange(List<FeeService> feeServices)
    {
    }

    bool HasData();

    Task<FeeService?> GetByIdAsync(FeeServiceId id, CancellationToken cancellationToken = default);

    IQueryable<FeeService> GetEntitiesAsQueryable();
    public void Update(FeeService feeService);

    void Remove(FeeService feeService);

    Task<PagedList<FeeService>> GetAllPaged(PagedQuery<FeeService, FeeServiceId> request,
        IQueryable<FeeService>? queryable = null);
}