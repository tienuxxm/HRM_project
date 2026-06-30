using Application.Products.GetOne;

namespace Application.Products.GetAllWithCategory;

public class ProductWithCategoryResponse
{
    public string CategoryName { get; set; }
    public List<ProductResponse> Products { get; set; }
}