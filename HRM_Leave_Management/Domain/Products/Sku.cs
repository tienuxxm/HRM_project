namespace Domain.Products;

// Stock Keeping Unit
public record Sku
{
    private Sku(string value) => Value = value;

    public string Value { get; init; }

    public static explicit operator string(Sku sku) => sku.Value;

    public static Sku Create(string value)
    {
        return string.IsNullOrEmpty(value) ? new Sku("0") : new Sku(value);
    }
}