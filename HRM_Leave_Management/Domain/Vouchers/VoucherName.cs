namespace Domain.Vouchers;

public record VoucherName(string Value) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            VoucherName TValue => TValue.Value.CompareTo(Value),
            _ => 1
        };
    }
}