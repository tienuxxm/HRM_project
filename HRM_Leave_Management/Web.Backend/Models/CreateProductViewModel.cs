using System.ComponentModel.DataAnnotations;
using Application.Categories.GetOne;
using Application.Restaurants.GetAll;

namespace Web.Backend.Models;

public class CreateProductViewModel
{
    [Required] public Guid CategoryId { get; set; }

    /*[Required] public Guid RestaurantId { get; set; }*/
    [Required] [MaxLength(250)] public string Name { get; set; }
    [Required] [MaxLength(64)] public string Sku { get; set; }
    [Required] public decimal Price { get; set; }
    public IFormFile ImageFile { get; set; }
    public List<CategoryResponse> CategoryList { get; set; }
    public List<RestaurantResponse> RestaurantList { get; set; }
    public bool AllowDelivery { get; set; }
}