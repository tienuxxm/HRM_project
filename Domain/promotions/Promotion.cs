using Domain.Abstractions;
using Domain.PromotionToRestaurants;
using Domain.Restaurants;
using Domain.Shared;

namespace Domain.Promotions;

public sealed class Promotion : Entity<PromotionId>
{
    private Promotion(
        PromotionId id,
        PromotionName promotionName,
        PromotionTitle title,
        PromotionContent content,
        DateTime startedDate,
        DateTime endedDate,
        DateTime createdDate,
        ImageUrl? imageUrl) : base(id)
    {
        Id = id;
        PromotionName = promotionName;
        Title = title;
        Content = content;
        CreatedDate = createdDate;
        StartedAt = startedDate;
        EndedAt = endedDate;
        ImageUrl = imageUrl;
    }

    public Promotion()
    {
    }

    public PromotionName PromotionName { get; private set; }

    public PromotionTitle Title { get; private set; }

    public PromotionContent Content { get; private set; }

    public ImageUrl? ImageUrl { get; private set; }

    public DateTime CreatedDate { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime EndedAt { get; private set; }
    public List<PromotionToRestaurant> PromotionToRestaurants { get; set; }

    public static Promotion Create(PromotionName name, PromotionTitle title, PromotionContent content,
        DateTime startedAt, DateTime endedAt, DateTime createdDate, ImageUrl? imageUrl)
    {
        return new Promotion(PromotionId.New(), name, title, content, startedAt, endedAt, createdDate, imageUrl);
    }


    public void Update(PromotionName name, PromotionTitle title, PromotionContent content, DateTime? startedAt,
        DateTime? endedAt, ImageUrl? imageUrl)
    {
        PromotionName = name;
        Title = title;
        Content = content;
        StartedAt = startedAt ?? StartedAt;
        EndedAt = endedAt ?? EndedAt;
        ImageUrl = imageUrl ?? ImageUrl;
    }
}