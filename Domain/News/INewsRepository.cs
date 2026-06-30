using Domain.Abstractions;

namespace Domain.News;

public interface INewsRepository
{
    Task<News?> GetByIdAsync(NewsId id, CancellationToken cancellationToken = default);
    void Add(News news);
    void Remove(News news);

    public IQueryable<News> GetEntitiesAsQueryable();
    public Task<PagedList<News>> GetAllPaged(PagedQuery<News, NewsId> request, IQueryable<News>? queryable = null);
    public void Update(News news);
}