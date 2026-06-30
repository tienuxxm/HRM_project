using Application.RestaurantArea.GetAll;

namespace Web.Backend.Models;

public class ManageRestaurantViewModel
{
    public string PageTitle { get; set; }
    public ManageRestaurantModel ManageRestaurantModel { get; set; }
    public List<RestaurantAreaResponse> RestaurantAreas { get; set; }
}

public class RestaurantSearchModel
{
    public string SearchValue { get; set; }
}

public class ManageRestaurantModel
{
    public Guid? Id { get; set; }
    public string? RestaurantName { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? OpeningAt { get; set; }
    public string? ClosingAt { get; set; }
    public string? MapLink { get; set; }
    public IFormFile? ImageFile { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? AreaId { get; set; }

    public TimeOnly OpeningAtValue
    {
        get
        {
            var timeSplit = OpeningAt.Split(":");
            var hour = Int32.Parse(timeSplit[0]);
            var minute = Int32.Parse(timeSplit[1]);
            return new TimeOnly(hour, minute);
        }
    }

    public TimeOnly ClosingAtValue
    {
        get
        {
            var timeSplit = ClosingAt.Split(":");
            var hour = Int32.Parse(timeSplit[0]);
            var minute = Int32.Parse(timeSplit[1]);
            return new TimeOnly(hour, minute);
        }
    }
}