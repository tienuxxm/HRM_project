using Domain.Abstractions;
using Domain.Products;
using Domain.Restaurants;

namespace Domain.ProductOfRestaurants;

public class ProductOfRestaurant : Entity<ProductRestaurantId>
{
    private ProductOfRestaurant()
    {
    }

    private ProductOfRestaurant(ProductRestaurantId id, ProductId productId, RestaurantId restaurantId,
        bool allowDelivery) : base(id)
    {
        ProductId = productId;
        RestaurantId = restaurantId;
        Quantity = 0;
        AllowDelivery = allowDelivery;
        Tags = "";
        IsActive = true;
    }

    public static ProductOfRestaurant Create(ProductId productId, RestaurantId restaurantId, bool allowDelivery)
    {
        return new ProductOfRestaurant(ProductRestaurantId.New, productId, restaurantId, allowDelivery);
    }

    public void Disable()
    {
        IsActive = true;
    }

    public ProductId ProductId { get; private set; }
    public RestaurantId RestaurantId { get; private set; }
    public Product Product { get; private set; }
    public Restaurant Restaurant { get; private set; }
    public bool IsActive { get; private set; }
    public int Quantity { get; private set; }
    public bool AllowDelivery { get; private set; }
    public string Tags { get; private set; }
}