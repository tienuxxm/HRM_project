using System.Globalization;
using Application.Categories.GetOne;
using Application.Restaurants.GetAll;

namespace Application.Products.GetOne;

public sealed class ProductResponse
{
    public Guid Id { get; init; }
    public string ProductName { get; init; }
    public CategoryResponse CategoryResponse { get; init; }
    public string SKU { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; }
    public DateTime CreatedDate { get; init; }
    public string? ImageUrl { get; set; }

    public bool AllowDelivery { get; set; }
    /*public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; }*/

    public string PriceDisplay =>
        Price.ToString("#,###", CultureInfo.GetCultureInfo("vi-VN").NumberFormat) + " " + Currency;
}