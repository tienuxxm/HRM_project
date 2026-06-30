namespace Domain.Shared;

public record Email(string Value) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            Email TValue => TValue.Value.CompareTo(Value),
            _ => 1
        };
    }
}