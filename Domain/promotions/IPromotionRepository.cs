using System.Linq.Expressions;
using Domain.Abstractions;
using Domain.Members;
using Domain.Promotions;

namespace Domain.Promotions;

public interface IPromotionRepository
{
    Task<Promotion?> GetByIdAsync(PromotionId id, CancellationToken cancellationToken = default);
   
    void Add(Promotion promotion);

    void Update(Promotion promotion);

    void Remove(Promotion promotion);

    IQueryable<Promotion> GetEntitiesAsQueryable();

    Task<List<Promotion>?> GetAll(
        CancellationToken cancellationToken = default);

    Task<List<Promotion>?> Search(string searchValue, CancellationToken cancellationToken);

    public Task<PagedList<Promotion>> GetAllPaged(PagedQuery<Promotion, PromotionId> request,
        IQueryable<Promotion>? queryable = null);
}