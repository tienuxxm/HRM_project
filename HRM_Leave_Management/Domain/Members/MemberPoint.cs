namespace Domain.Members;

public record MemberPoint(int Value) : IComparable
{
    public static MemberPoint operator +(MemberPoint first, MemberPoint second)
    {
        return new MemberPoint(first.Value + second.Value);
    }

    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            LastName TValue => TValue.Value.CompareTo(Value),
            _ => 1
        };
    }
};