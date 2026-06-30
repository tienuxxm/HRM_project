using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Bookings;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Application.Bookings.BookingReport;

public class GetBookingReportCommandHandler : ICommandHandler<GetBookingReportCommand, BookingReportResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetBookingReportCommandHandler(IBookingRepository bookingRepository, IDateTimeProvider dateTimeProvider,
        IOrderRepository orderRepository)
    {
        _bookingRepository = bookingRepository;
        _dateTimeProvider = dateTimeProvider;
        _orderRepository = orderRepository;
    }

    public async Task<Result<BookingReportResponse>> Handle(GetBookingReportCommand request,
        CancellationToken cancellationToken)
    {
        var currentMonth = _dateTimeProvider.UtcNow.Month;

        var totalCompleteBooking = await GetTotalBooking(currentMonth, BookingStatus.Completed, cancellationToken);
        var totalCompleteBookingLastMonth =
            await GetTotalBooking(currentMonth - 1, BookingStatus.Completed, cancellationToken);
        var completeBookingGrowthRate = CalculateRate(totalCompleteBooking, totalCompleteBookingLastMonth);

        var totalInProgressBooking = await GetTotalBooking(currentMonth, BookingStatus.Confirmed, cancellationToken);
        var totalInProgressBookingLastMonth =
            await GetTotalBooking(currentMonth - 1, BookingStatus.Confirmed, cancellationToken);
        var inProgressBookingGrowthRate = CalculateRate(totalInProgressBooking, totalInProgressBookingLastMonth);

        var totalCancelBooking = await GetTotalBooking(currentMonth, BookingStatus.Cancelled, cancellationToken) +
                                 await GetTotalBooking(currentMonth, BookingStatus.Rejected, cancellationToken);
        var totalCancelBookingLastMonth =
            await GetTotalBooking(currentMonth - 1, BookingStatus.Cancelled, cancellationToken)
            + await GetTotalBooking(currentMonth - 1, BookingStatus.Rejected, cancellationToken);
        var cancelBookingGrowthRate = CalculateRate(totalCancelBooking, totalCancelBookingLastMonth);

        var totalCompleteOrder =
            await GetTotalOrder(currentMonth, new List<OrderStatus> { OrderStatus.Done }, cancellationToken);
        var totalInProgressOrder = await GetTotalOrder(currentMonth,
            new List<OrderStatus> { OrderStatus.Process, OrderStatus.Shipping }, cancellationToken);
        var totalCancelOrder =
            await GetTotalOrder(currentMonth, new List<OrderStatus> { OrderStatus.Cancel }, cancellationToken);

        return Result.Success(new BookingReportResponse()
        {
            BookingSuccessReport = new BookingReport()
            {
                GrowthRate = completeBookingGrowthRate,
                TotalQuantity = totalCompleteBooking
            },
            BookingCancelReport = new BookingReport()
            {
                GrowthRate = cancelBookingGrowthRate,
                TotalQuantity = totalCancelBooking
            },
            BookingInProcessReport = new BookingReport()
            {
                GrowthRate = inProgressBookingGrowthRate,
                TotalQuantity = totalInProgressBooking
            },
            TotalCompleteOrder = totalCompleteOrder,
            TotalCancelOrder = totalCancelOrder,
            TotalInProgressOrder = totalInProgressOrder
        });
    }

    private int CalculateRate(int totalCurrentMonth, int totalLastMonth) =>
        totalLastMonth == 0 ? totalCurrentMonth * 100 : (totalCurrentMonth - totalLastMonth) / totalLastMonth * 100;

    private async Task<int> GetTotalBooking(int month, BookingStatus status, CancellationToken cancellationToken)
    {
        return await _bookingRepository.GetEntitiesAsQueryable()
            .CountAsync(
                x => x.CreateDate.Month == month && x.Status == status,
                cancellationToken);
    }

    private async Task<int> GetTotalOrder(int month, List<OrderStatus> statuses, CancellationToken cancellationToken)
    {
        return await _orderRepository.GetEntitiesAsQueryable()
            .CountAsync(
                x => x.CreatedDate.Month == month && statuses.Contains(x.Status),
                cancellationToken);
    }
}