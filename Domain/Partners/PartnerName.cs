namespace Domain.Partners;

public record PartnerName(string Value) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            PartnerName TValue => TValue.Value.CompareTo(Value),
            _ => 1
        };
    }
}