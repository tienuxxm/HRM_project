using Domain.Abstractions;
using Domain.Products;
using Domain.RestaurantMenus;

namespace Domain.RestaurantMenuProducts;

public class RestaurantMenuProduct : Entity<RestaurantMenuProductId>
{
    private RestaurantMenuProduct()
    {
    }

    private RestaurantMenuProduct(RestaurantMenuProductId id, RestaurantMenuId restaurantMenuId, ProductId productId,
        int quantity, bool allowDelivery) : base(id)
    {
        RestaurantMenuId = restaurantMenuId;
        Quantity = quantity;
        ProductId = productId;
        AllowDelivery = allowDelivery;
        IsActive = true;
    }

    public RestaurantMenuId RestaurantMenuId { get; private set; }
    public RestaurantMenu RestaurantMenu { get; private set; }
    public ProductId ProductId { get; private set; }
    public Product Product { get; private set; }
    public int Quantity { get; private set; }
    public bool IsActive { get; private set; }
    public bool AllowDelivery { get; private set; }
}