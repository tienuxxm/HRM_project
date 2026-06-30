namespace Domain.News;

public record NewsId(Guid Value)
{
    public static NewsId New() => new(Guid.NewGuid());
}