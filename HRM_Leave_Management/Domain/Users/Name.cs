namespace Domain.Users;

public record Name(string Value) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            Name name => name.Value.CompareTo(Value),
            _ => 1
        };
    }
}