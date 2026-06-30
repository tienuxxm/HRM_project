using Domain.Abstractions;

namespace Domain.Restaurants;

public interface IRestaurantRepository
{
    Task<Restaurant?> GetByIdAsync(RestaurantId id, CancellationToken cancellationToken = default);

    void Add(Restaurant entity);

    Task<List<Restaurant>?> GetAll(CancellationToken cancellationToken);
    Task<List<Restaurant>?> GetAllWithOutDeleted(CancellationToken cancellationToken);
    Task<List<Restaurant>?> GetByIdsAsync(List<RestaurantId> ids, CancellationToken cancellationToken);

    void Remove(Restaurant entity);
    void Update(Restaurant entity);

    public Task<PagedList<Restaurant>> GetAllPaged(PagedQuery<Restaurant, RestaurantId> request,
        IQueryable<Restaurant>? queryable = null);

    public IQueryable<Restaurant> GetEntitiesAsQueryable();
}