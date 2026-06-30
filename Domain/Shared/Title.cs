namespace Domain.Shared;

public record Title(string Value) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            Title TValue => TValue.Value.CompareTo(Value),
            _ => 1
        };
    }
}