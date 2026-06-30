using Application.Bookings.GetBooking;
using Domain.Abstractions;

namespace Web.Backend.Models;

public class BookingViewModel
{
    public PagedList<BookingResponse> Response { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortColumn { get; set; }
    public string? SortOrder { get; set; }
}