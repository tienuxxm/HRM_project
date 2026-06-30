namespace Domain.Restaurants;

public record RestaurantName(string Value) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            RestaurantName TValue => TValue.Value.CompareTo(Value),
            _ => 1
        };
    }
}