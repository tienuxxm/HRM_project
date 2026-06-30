using Application.Abstractions.Messaging;
using Application.Orders.Create;

namespace Application.Bookings.UpdateBooking;

public record UpdateBookingCommand(
    Guid BookingId,
    Guid RestaurantId,
    string TotalOfPeople,
    string PhoneNumeber,
    string FullName,
    List<CreateLineItem>? LineItems,
    string? Note,
    DateTime BookingDate) : ICommand;