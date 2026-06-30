using Application.Extensions;
using Application.Orders.GetOrder;
using Domain.Bookings;

namespace Application.Bookings.GetBooking;

public sealed class BookingResponse
{
    public Guid Id { get; init; }
    public string IdWrapped => "\"" + Id + "\"";

    public Guid RestaurantId { get; init; }
    public string RestaurantName { get; init; }

    public BookingStatus Status { get; init; }

    public string TotalOfPeople { get; init; }

    public DateTime BookingTime { get; init; }

    public DateTime CreatedDate { get; init; }
    public string? Note { get; set; }
    public string BookingCode { get; init; }
    public string MemberName { get; init; }
    public string? MemberAvatar { get; init; }
    public string? RestaurantAddress { get; init; }
    public string? PhoneNumber { get; set; }
    public List<LineItemResponse>? LineItemResponses { get; set; }

    public string BookingStatusDisplay => Status.GetDescription();
}