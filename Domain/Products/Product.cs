using Domain.Abstractions;
using Domain.Categories;
using Domain.Shared;

namespace Domain.Products;

public sealed class Product : Entity<ProductId>
{
    private Product(ProductId id, CategoryId categoryId, ProductName productName,
        Money price, ImageUrl imageUrl, DateTime createdDate, bool allowDelivery = false)
    {
        Id = id;
        ProductName = productName;
        Price = price;
        CategoryId = categoryId;
        CreatedDate = createdDate;
        ImageUrl = imageUrl;
        AllowDelivery = allowDelivery;
        IsDeleted = false;
    }

    public Product()
    {
    }

    public CategoryId CategoryId { get; private set; }
    public Category Category { get; private set; } = null;

    public ProductName ProductName { get; private set; }

    public Money Price { get; private set; }

    public Sku? Sku { get; }
    public ImageUrl ImageUrl { get; private set; }
    public bool AllowDelivery { get; private set; }
    public bool? IsDeleted { get; private set; }

    public DateTime CreatedDate { get; private set; }

    public static Product Create(CategoryId categoryId, ProductName productName, Money price, ImageUrl imageUrl,
        DateTime createdDate, bool allowDelivery = false)
    {
        return new Product(ProductId.New(), categoryId, productName, price, imageUrl, createdDate, allowDelivery);
    }

    public void Update(ProductName? productName, Money? price, CategoryId? categoryId, ImageUrl? imageUrl,
        bool? allowDelivery = false)
    {
        ProductName = productName ?? ProductName;
        Price = price ?? Price;
        CategoryId = categoryId ?? CategoryId;
        ImageUrl = imageUrl ?? ImageUrl;
        AllowDelivery = allowDelivery.HasValue ? allowDelivery.Value : AllowDelivery;
    }

    public void Delete()
    {
        IsDeleted = true;
    }
}