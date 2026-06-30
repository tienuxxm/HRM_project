namespace Domain.Vouchers;

public record Place(string Value) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            Place TValue => TValue.Value.CompareTo(Value),
            _ => 1
        };
    }
}