using Domain.Abstractions;

namespace Domain.PromotionToRestaurants;

public interface IPromotionToRestaurantRepository
{
    Task<PromotionToRestaurant?> GetByIdAsync(PromotionToRestaurantId id);

    void Add(PromotionToRestaurant promotionToRestaurant);

    void Update(PromotionToRestaurant promotionToRestaurant);

    void Remove(PromotionToRestaurant PromotionToRestaurant);
    
    void AddRange(List<PromotionToRestaurant>  PromotionToRestaurant);
    
    IQueryable<PromotionToRestaurant> GetProductsAsync();
    
    void RemoveRange(List<PromotionToRestaurant> promotionToRestaurants);
    
    IQueryable<PromotionToRestaurant> GetEntitiesAsQueryable();

    public Task<PagedList<PromotionToRestaurant>> GetAllPaged(PagedQuery<PromotionToRestaurant, PromotionToRestaurantId> request,
        IQueryable<PromotionToRestaurant>? queryable = null);

    Task<List<PromotionToRestaurant>?> GetAll(
        CancellationToken cancellationToken = default);

    Task<List<PromotionToRestaurant>?> GetByIdsAsync(
        List<PromotionToRestaurant> ids,
        CancellationToken cancellationToken = default);
}