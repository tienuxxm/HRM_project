using Domain.Abstractions;

namespace Domain.Restaurants;

public class RestaurantErrors
{
    public static Error NotFound = new(
        "Property.Found",
        "The property with the specified identifier was not found");
}