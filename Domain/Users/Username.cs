namespace Domain.Users;

public record Username(string Value) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            Username username => username.Value.CompareTo(Value),
            _ => 1
        };
    }
}