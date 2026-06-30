namespace Domain.Shared;

public record Description(string Value) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            Description TValue => TValue.Value.CompareTo(Value),
            _ => 1
        };
    }
}