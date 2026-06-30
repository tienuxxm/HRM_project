namespace Application.Restaurants.Search;

public sealed class RestaurantResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; }

    public string Description { get; init; }

    public AddressResponse Address { get; set; }
}