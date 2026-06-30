namespace Domain.Vouchers;

public record TitleVoucher(string Value) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            TitleVoucher TValue => TValue.Value.CompareTo(Value),
            _ => 1
        };
    }
}