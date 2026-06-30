using Application.Categories.GetOne;
using Application.Products.GetOne;
using Application.Restaurants.GetAll;

namespace Web.Backend.Models;

public class ProductDetailViewModel
{
    public ProductResponse ProductDetail { get; set; }
    public List<CategoryResponse> CategoryResponses { get; set; }
    public List<RestaurantResponse> RestaurantResponses { get; set; }
    public ProductUpdateModel ProductUpdateModel { get; set; }
}

public class ProductUpdateModel
{
    public Guid Id { get; set; }
    public string? ProductName { get; set; }
    public decimal? ProductPrice { get; set; }
    public string? SKU { get; set; }
    public IFormFile? ImageFile { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? RestaurantId { get; set; }
    public bool? AllowDelivery { get; set; }
}