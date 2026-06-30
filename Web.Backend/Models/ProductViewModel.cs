using Application.Products.GetAllPaged;

namespace Web.Backend.Models;

public class ProductViewModel
{
    public GetProductsResponse ProductsResponse { get; set; }
    public bool IsDeliveryMenu { get; set; }
}