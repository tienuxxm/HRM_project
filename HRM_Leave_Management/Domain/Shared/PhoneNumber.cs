namespace Domain.Shared;

public record PhoneNumber(string Value) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            PhoneNumber TValue => TValue.Value.CompareTo(Value),
            _ => 1
        };
    }
}