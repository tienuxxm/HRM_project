using Domain.Abstractions;
using Domain.Shared;

namespace Domain.News;

public class News : Entity<NewsId>
{
    private News(NewsId id, Content content, Title title,
        Description description, ImageUrl thumbnail, DateTime createdDate) : base(id)
    {
        Content = content;
        Title = title;
        Description = description;
        Thumbnail = thumbnail;
        CreatedDate = createdDate;
    }

    public DateTime CreatedDate { get; private set; }
    public Content Content { get; private set; }
    public Title Title { get; private set; }
    public Description Description { get; private set; }
    public ImageUrl Thumbnail { get; private set; }

    public static News Create(Content content, Title title, Description description,
        ImageUrl thumbnail, DateTime createdDate)
    {
        return new News(NewsId.New(), content, title, description, thumbnail, createdDate);
    }

    public void Update(Content content, Title title, Description description,
        ImageUrl thumbnail)
    {
        Content = content;
        Description = description;
        Title = title;
        Thumbnail = thumbnail;
    }
}