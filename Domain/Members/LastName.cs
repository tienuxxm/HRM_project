namespace Domain.Members;

public record LastName(string Value) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            LastName TValue => TValue.Value.CompareTo(Value),
            _ => 1
        };
    }
}