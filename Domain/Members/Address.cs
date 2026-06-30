namespace Domain.Members;

public record Address(string Value) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            Address TValue => TValue.Value.CompareTo(Value),
            _ => 1
        };
    }
}