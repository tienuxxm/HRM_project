using Application.Abstractions.Messaging;

namespace Application.Bookings.BookingReport;

public record GetBookingReportCommand() : ICommand<BookingReportResponse>;