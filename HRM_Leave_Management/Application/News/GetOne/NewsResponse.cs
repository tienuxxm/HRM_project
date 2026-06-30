namespace Application.News.GetOne;

public class NewsResponse
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Thumbnail { get; set; }

    public string ThumbNailId { get; set; }
    public DateTime? CreatedDate { get; set; }
}