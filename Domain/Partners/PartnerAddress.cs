namespace Domain.Partners;

public record PartnerAddress(string Value) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            PartnerAddress TValue => TValue.Value.CompareTo(Value),
            _ => 1
        };
    }
}