using System.Linq.Expressions;
using Domain.Abstractions;

namespace Domain.Categories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(CategoryId id, CancellationToken cancellationToken = default);

    void Add(Category category);

    public void Update(Category category);
    public IQueryable<Category> GetEntitiesAsQueryable();

    public Task<PagedList<Category>> GetAllPaged(PagedQuery<Category, CategoryId> request,
        IQueryable<Category>? queryable = null);


    Task<bool> IsExistedAsync(Expression<Func<Category, bool>> expression,
        CancellationToken cancellationToken = default);

    void Remove(Category entity);

    Task<List<Category>?> GetAll(
        CancellationToken cancellationToken = default);
}