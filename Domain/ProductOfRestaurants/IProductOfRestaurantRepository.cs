using Domain.Abstractions;

namespace Domain.ProductOfRestaurants;

public interface IProductOfRestaurantRepository
{
    void Add(ProductOfRestaurant productOfRestaurant);
    void AddRange(List<ProductOfRestaurant> productOfRestaurant);
    public IQueryable<ProductOfRestaurant> GetEntitiesAsQueryable();

    public Task<PagedList<ProductOfRestaurant>> GetAllPaged(
        PagedQuery<ProductOfRestaurant, ProductRestaurantId> request,
        IQueryable<ProductOfRestaurant>? queryable = null);
}