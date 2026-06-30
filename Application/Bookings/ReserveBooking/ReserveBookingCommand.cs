using Application.Abstractions.Messaging;
using Domain.Bookings;

namespace Application.Bookings.ReserveBooking;

public record ReserveBookingCommand(
    Guid RestaurantId,
    string TotalOfPeople,
    string PhoneNumber,
    string FullName,
    string? Note,
    DateTime BookingDate,
    bool Save = true,
    bool createdBySystem = false) : ICommand<Booking>;