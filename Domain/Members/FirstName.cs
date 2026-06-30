namespace Domain.Members;

public record FirstName(string Value) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            FirstName TValue => TValue.Value.CompareTo(Value),
            _ => 1
        };
    }
}