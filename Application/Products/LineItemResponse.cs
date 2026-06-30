using Domain.Shared;

namespace Application.Products;

public class LineItemResponse
{
    public Money Price { get; set; }
    public int Quantity { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
}