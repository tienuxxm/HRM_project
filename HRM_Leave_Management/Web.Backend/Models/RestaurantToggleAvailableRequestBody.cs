namespace Web.Backend.Models;

public class RestaurantToggleAvailableRequestBody
{
    public Guid Id { get; set; }
    public bool Toggle { get; set; }
}