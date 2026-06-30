using FluentValidation;

namespace Application.Bookings.ReserveBooking;

public class ReserveBookingCommandValidator : AbstractValidator<ReserveBookingCommand>
{
    public ReserveBookingCommandValidator()
    {
        RuleFor(c => c.RestaurantId).NotEmpty();
        RuleFor(c => c.BookingDate).NotEmpty();
        RuleFor(c => c.TotalOfPeople).NotEmpty();
    }
}