using Domain.Abstractions;

namespace Domain.Bookings;

public static class BookingErrors
{
    public static Error NotFound = new(
        "Booking.Found",
        "The booking with the specified identifier was not found");

    public static Error NoPermission = new(
        "Booking.NoPermission",
        "Don't have permission");

    public static Error Overlap = new(
        "Booking.Overlap",
        "The current booking is overlapping with an existing one");

    public static Error NotReserved = new(
        "Booking.NotReserved",
        "The booking is not pending");

    public static Error NotConfirmed = new(
        "Booking.NotReserved",
        "The booking is not confirmed");

    public static Error AlreadyStarted = new(
        "Booking.AlreadyStarted",
        "The booking has already started");

    public static Error RestaurantUnavailable = new(
        "Booking.Unavailable",
        "Chi nhánh đã full bàn, vui lòng liên hệ Hotline 1900 6096 để được hỗ trợ");
}