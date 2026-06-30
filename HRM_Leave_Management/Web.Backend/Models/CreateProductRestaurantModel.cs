namespace Web.Backend.Models;

public class CreateProductRestaurantModel
{
    public Guid RestaurantId { get; set; }
    public List<Guid> ProductIds { get; set; }
}