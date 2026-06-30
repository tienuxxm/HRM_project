namespace Application.Restaurants.GetAll;

public sealed class RestaurantResponse
{
    public Guid Id { get; set; }
    public string RestaurantName { get; set; }
    public AddressResponse Address { get; set; }
    public DateTime? CreateDate { get; set; }
    public string AreaName { get; set; }
    public string OpeningAt { get; set; }
    public string ClosingAt { get; set; }
    public string? ImageUrl { get; set; }
    public Guid AreaId { get; set; }
    public string? MapLink { get; set; }
    public bool IsAvailable { get; set; }
}

public sealed class AddressResponse
{
    public string Country { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string AddressCombine => Street + " " + State + " " + City;
}