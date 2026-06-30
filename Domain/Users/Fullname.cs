namespace Domain.Users;

public record Fullname(string Value) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            Fullname fullname => fullname.Value.CompareTo(Value),
            _ => 1
        };
    }
}