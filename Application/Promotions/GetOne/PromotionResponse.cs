using Application.Restaurants.GetAll;

namespace Application.Promotions.GetOne;

public sealed class PromotionResponse
{
    public Guid Id { get; init; }
    public string PromotionName { get; init; }
    public string Title { get; init; }
    public string Content { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime EndedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public string ImageUrl { get; set; }
    public List<RestaurantResponse> Restaurants { get; set; }
}