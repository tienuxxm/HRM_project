using Domain.Abstractions;

namespace Domain.News;

public static class NewsError
{
    public static Error NotFound => new("News.NotFound", "The news with the specified identifier was not found");
}