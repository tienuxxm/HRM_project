using System.Linq.Expressions;
using Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal abstract class Repository<TEntity, TEntityId>
    where TEntity : Entity<TEntityId>
    where TEntityId : class
{
    protected readonly ApplicationDbContext DbContext;

    protected Repository(ApplicationDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public IQueryable<TEntity> GetEntitiesAsQueryable()
    {
        return DbContext.Set<TEntity>().AsQueryable();
    }

    public Task<PagedList<TEntity>> GetAllPaged(PagedQuery<TEntity, TEntityId> request,
        IQueryable<TEntity>? queryable = null)
    {
        var entityQuery = queryable ?? GetEntitiesAsQueryable();

        if (request.SortExpression is null || string.IsNullOrEmpty(request.SortColumn))
            return Task.FromResult(PagedList<TEntity>.ToPagedList(entityQuery, request.Page, request.PageSize));
        entityQuery = request.OrderByType == OrderByType.DESC
            ? entityQuery.OrderByDescending(request.SortExpression)
            : entityQuery.OrderBy(request.SortExpression);

        return Task.FromResult(PagedList<TEntity>.ToPagedList(entityQuery, request.Page, request.PageSize));
    }

    public async Task<bool> IsExistedAsync(Expression<Func<TEntity, bool>> expression,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<TEntity>().AnyAsync(expression, cancellationToken);
    }

    public async Task<TEntity?> GetByIdAsync(
        TEntityId id,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<TEntity>()
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task<List<TEntity>?> GetByIdsAsync(
        List<TEntityId> ids,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<TEntity>()
            .Where(t => ids.Contains(t.Id)).ToListAsync(cancellationToken);
    }

    public async Task<List<TEntity>?> GetAll(
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<TEntity>()
            .ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> GetLatestByProperty(Expression<Func<TEntity, dynamic>> expression,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<TEntity>().OrderByDescending(expression).FirstOrDefaultAsync(cancellationToken);
    }


    public void Add(TEntity entity)
    {
        DbContext.Add(entity);
    }

    public void AddRange(List<TEntity> entities)
    {
        DbContext.AddRange(entities);
    }

    public void Remove(TEntity entity)
    {
        DbContext.Remove(entity);
    }

    public void RemoveRange(List<TEntity> entities)
    {
        DbContext.RemoveRange(entities);
    }

    public void Update(TEntity entity)
    {
        DbContext.Update(entity);
    }

    public void UpdateRange(List<TEntity> entities)
    {
        DbContext.UpdateRange(entities);
    }
}