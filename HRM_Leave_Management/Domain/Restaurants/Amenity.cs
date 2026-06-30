using System.ComponentModel;

namespace Domain.Restaurants;

public enum Amenity
{
    [Description("Wifi")] WiFi = 1,
    [Description("Air Conditioning")] AirConditioning = 2,
    [Description("Parking")] Parking = 3,
    [Description("Pet Friendly")] PetFriendly = 4,
    [Description("Swimming Pool")] SwimmingPool = 5,
    [Description("Gym")] Gym = 6,
    [Description("Spa")] Spa = 7,
    [Description("Terrace")] Terrace = 8,
    [Description("Mountain View")] MountainView = 9,
    [Description("Garden View")] GardenView = 10
}