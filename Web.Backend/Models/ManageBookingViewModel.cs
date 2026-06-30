using Application.Members.Responses;
using Application.Products.GetOne;
using Application.Restaurants.GetAll;

namespace Web.Backend.Models;

public class ManageBookingViewModel
{
    //public List<MemberResponse> MemberList { get; set; }
    public List<RestaurantResponse> RestaurantList { get; set; }
    public List<ProductResponse> ProductList { get; set; }
    public BookingModel BookingModel { get; set; }
    public string Title { get; set; }

    public List<string> ListNumberOfPeople { get; set; } = new List<string>()
    {
        "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11-15", "16-20", "20+"
    };
}

public class BookingModel
{
    public Guid? Id { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public Guid RestaurantId { get; set; }
    public List<BookingLineItemModel>? LineItems { get; set; }
    public string NumberOfPeople { get; set; }
    public string? Note { get; set; }
    public string? Date { get; set; }
    public string? Time { get; set; }
    public string DateAndTime => Date + " " + Time;
}

public class BookingLineItemModel
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
}