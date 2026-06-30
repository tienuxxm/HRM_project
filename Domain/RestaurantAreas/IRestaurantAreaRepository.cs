using Domain.Abstractions;

namespace Domain.RestaurantAreas;

public interface IRestaurantAreaRepository
{
    Task<List<RestaurantArea>?> GetAll(CancellationToken cancellationToken = default);
    public IQueryable<RestaurantArea> GetEntitiesAsQueryable();

    public Task<PagedList<RestaurantArea>> GetAllPaged(PagedQuery<RestaurantArea, RestaurantAreaId> request,
        IQueryable<RestaurantArea>? queryable = null);

    void Add(RestaurantArea restaurantArea);

    void Update(RestaurantArea restaurantArea);

    Task<RestaurantArea?> GetByIdAsync(
        RestaurantAreaId id,
        CancellationToken cancellationToken = default);
}