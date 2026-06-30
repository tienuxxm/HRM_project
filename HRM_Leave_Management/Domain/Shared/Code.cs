namespace Domain.Shared;

public record Code(string Value) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            Code TValue => TValue.Value.CompareTo(Value),
            _ => 1
        };
    }
}