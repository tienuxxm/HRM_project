using Domain.Abstractions;
using Domain.Promotions;
using Domain.Restaurants;

namespace Domain.PromotionToRestaurants;

public sealed class PromotionToRestaurant : Entity<PromotionToRestaurantId>
{
    private PromotionToRestaurant(
        PromotionToRestaurantId id,
        PromotionId promotionId,
        RestaurantId restaurantId,
        Promotion promotion
    ) : base(id)
    {
        Id = id;

        PromotionId = promotionId;
        RestaurantId = restaurantId;
        Promotion = promotion;
    }

    public PromotionToRestaurant()
    {
    }


    public PromotionId PromotionId { get; private set; }
    public Promotion Promotion { get; private set; }

    public RestaurantId RestaurantId { get; private set; }
    public Restaurant Restaurant { get; private set; }

    public static PromotionToRestaurant Create(PromotionId promotionId, RestaurantId restaurantId, Promotion promotion)
    {
        return new PromotionToRestaurant(PromotionToRestaurantId.New(), promotionId, restaurantId, promotion);
    }
}