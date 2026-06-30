namespace Domain.Bookings;

public record FullName(string Value) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            FullName TValue => TValue.Value.CompareTo(Value),
            _ => 1
        };
    }
}