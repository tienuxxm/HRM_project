using Domain.Abstractions;
using Domain.Orders;
using Domain.PromotionToRestaurants;
using Domain.RestaurantAreas;
using Domain.Shared;


namespace Domain.Restaurants;

public sealed class Restaurant : Entity<RestaurantId>
{
    private Restaurant(
        RestaurantId id,
        RestaurantAreaId restaurantAreaId,
        RestaurantName restaurantName,
        Address address,
        DateTime createdAt,
        TimeOnly openingAt,
        TimeOnly closingAt,
        ImageUrl imageUrl,
        string mapLink)
        : base(id)
    {
        RestaurantAreaId = restaurantAreaId;
        RestaurantName = restaurantName;
        Address = address;
        CreateDate = createdAt;
        OpeningAt = openingAt;
        ClosingAt = closingAt;
        ImageKey = imageUrl;
        MapLink = mapLink;
        IsAvailable = true;
    }

    private Restaurant()
    {
    }

    public RestaurantArea RestaurantArea { get; private set; } = null;
    public RestaurantAreaId RestaurantAreaId { get; private set; } = null;
    public RestaurantName RestaurantName { get; private set; }
    public Address Address { get; private set; }
    public Operation Operation { get; private set; }
    public DateTime? CreateDate { get; private set; }
    public string? MapLink { get; private set; }
    public TimeOnly OpeningAt { get; private set; }
    public TimeOnly ClosingAt { get; private set; }
    public List<Order> Orders { get; private set; }
    public List<PromotionToRestaurant> PromotionToRestaurants { get; set; }
    public bool IsDeleted { get; private set; } = false;
    public ImageUrl? ImageKey { get; private set; }
    public bool IsAvailable { get; private set; }

    public void SetAvailable()
    {
        IsAvailable = true;
    }

    public void SetUnavailable()
    {
        IsAvailable = false;
    }

    public void Delete()
    {
        IsDeleted = true;
    }

    public static Restaurant Create(
        RestaurantAreaId restaurantAreaId,
        RestaurantName restaurantName,
        Address address,
        DateTime createdAt,
        TimeOnly openingAt,
        TimeOnly closingAt,
        ImageUrl imageUrl,
        string mapLink)
    {
        return new Restaurant(RestaurantId.New(), restaurantAreaId, restaurantName, address, createdAt, openingAt,
            closingAt, imageUrl, mapLink);
    }

    public void Update(
        RestaurantName? restaurantName,
        Address? address, TimeOnly? openingAt, TimeOnly? closingAt, RestaurantAreaId? restaurantAreaId,
        ImageUrl? imageKey, string? mapLink)
    {
        OpeningAt = openingAt.HasValue ? openingAt.Value : OpeningAt;
        ClosingAt = closingAt.HasValue ? closingAt.Value : ClosingAt;
        RestaurantName = restaurantName ?? RestaurantName;
        RestaurantAreaId = restaurantAreaId ?? restaurantAreaId;
        Address = address ?? Address;
        ImageKey = imageKey ?? ImageKey;
        MapLink = mapLink;
    }
}