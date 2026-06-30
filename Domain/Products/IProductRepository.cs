using Domain.Abstractions;
using Domain.Categories;

namespace Domain.Products;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(ProductId id);

    void Add(Product product);

    void Update(Product product);

    void Remove(Product product);
    IQueryable<Product> GetProductsAsync();
    Task<List<Product>?> GetProductsAsync(CategoryId categoryId);
    IQueryable<Product> GetEntitiesAsQueryable();

    public Task<PagedList<Product>> GetAllPaged(PagedQuery<Product, ProductId> request,
        IQueryable<Product>? queryable = null);

    Task<List<Product>?> GetAll(
        CancellationToken cancellationToken = default);

    Task<List<Product>?> GetByIdsAsync(
        List<ProductId> ids,
        CancellationToken cancellationToken = default);
}