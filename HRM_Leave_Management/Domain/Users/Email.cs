namespace Domain.Users;

public record Email(string Value) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            Email email => email.Value.CompareTo(Value),
            _ => 1
        };
    }
}