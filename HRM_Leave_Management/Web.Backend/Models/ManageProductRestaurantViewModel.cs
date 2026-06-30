using Application.Products.GetOne;
using Application.Restaurants.GetAll;
using Domain.Products;

namespace Web.Backend.Models;

public class ManageProductRestaurantViewModel
{
    public Guid? Id { get; set; }
    public Guid? RestaurantId { get; set; }
    public string? RestaurantName { get; set; }
    public string PageTitle { get; set; }
    public List<ProductId> ProductIds { get; set; } = new List<ProductId>();
    public List<RestaurantResponse> Restaurants { get; set; }
    public List<ProductResponse> Products { get; set; }
}