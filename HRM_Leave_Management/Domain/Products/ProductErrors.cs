using Domain.Abstractions;

namespace Domain.Products;

public class ProductErrors
{
    public static Error NotFound = new(
        "Product.Found",
        "The property with the specified identifier was not found");
}