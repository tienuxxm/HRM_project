using Domain.Abstractions;

namespace Domain.RestaurantAreas;

public class RestaurantAreaError
{
    public static Error CreateFail => new("Area.CrateFail", "Cannot create area");
    public static Error NotFound => new("Area.NotFound", "Area not found");
}