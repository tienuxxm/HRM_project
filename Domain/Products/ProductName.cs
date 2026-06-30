namespace Domain.Products;

public record ProductName(string Value) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            ProductName TValue => TValue.Value.CompareTo(Value),
            _ => 1
        };
    }
}