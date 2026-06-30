using Domain.Abstractions;
using Domain.Products;
using Domain.Shared;

namespace Domain.Orders;

public class LineItem : Entity<LineItemId>
{
    private LineItem(LineItemId id, OrderId orderId, ProductId? productId, ProductName productName, Money price,
        int quantity, Note? note, ImageUrl productImageUrl)
    {
        Id = id;
        OrderId = orderId;
        ProductId = productId;
        Price = price;
        ProductName = productName;
        Quantity = quantity;
        Note = note;
        ProductImageUrl = productImageUrl;
    }

    private LineItem()
    {
    }

    public static LineItem Create(OrderId orderId, ProductId? productId, ProductName productName, Money price,
        int quantity, Note? note, ImageUrl? productImage)
    {
        return new LineItem(LineItemId.New(), orderId, productId, productName, price, quantity, note, productImage);
    }

    public OrderId OrderId { get; private set; }
    public ProductName ProductName { get; private set; }
    public ImageUrl? ProductImageUrl { get; private set; }
    public ProductId? ProductId { get; private set; }
    public Money Price { get; private set; }
    public Note? Note { get; private set; }
    public int Quantity { get; private set; }
}