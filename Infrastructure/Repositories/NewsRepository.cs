using Domain.News;

namespace Infrastructure.Repositories;

internal sealed class NewsRepository : Repository<News, NewsId>, INewsRepository
{
    public NewsRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}