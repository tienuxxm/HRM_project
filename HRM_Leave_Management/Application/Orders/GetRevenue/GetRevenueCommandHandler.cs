using System.Globalization;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.GetRevenue;

public class GetRevenueCommandHandler : ICommandHandler<GetRevenueCommand, List<RevenueResponse>>
{
    private readonly IOrderRepository _orderRepository;

    public GetRevenueCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<List<RevenueResponse>>> Handle(GetRevenueCommand request,
        CancellationToken cancellationToken)
    {
        var firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
        var currentDate = DateTime.UtcNow;
        var difference = (7 + (currentDate.DayOfWeek - firstDayOfWeek)) % 7;
        var firstDay = currentDate.AddDays(-difference);
        var listDaysOfWeek = new List<DateTime>();
        var listMonth = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        for (var i = 1; i <= 7; i++)
        {
            var day = firstDay.AddDays(i);
            listDaysOfWeek.Add(day);
        }

        var query = _orderRepository.GetEntitiesAsQueryable()
            .Where(x => x.HasPayment).AsEnumerable();

        var totalRevenueByDays = listDaysOfWeek.Select(d =>
        {
            var total = query.Where(x => x.PaymentDate != null && x.PaymentDate.Value.Date == d.Date.Date)
                .Select(x => x.TotalBill.Amount)
                .Sum();
            return new { Date = d, Total = total, IsCurrent = d.Date == DateTime.UtcNow.Date };
        });
        var totalRevenueByMonth = listMonth.Select(d =>
        {
            var total = query.Where(x => x.PaymentDate != null && x.PaymentDate.Value.Month == d)
                .Select(x => x.TotalBill.Amount)
                .Sum();
            return new { Date = d, Total = total, IsCurrent = d == DateTime.UtcNow.Month };
        });
        var weeklyReportResponses = totalRevenueByDays.Select(x => new RevenueResponse()
        {
            Name = x.Date.ToString("dd/MM/yyyy"),
            Value = (int)x.Total,
            IsCurrent = x.IsCurrent,
            RevenueDataRangeType = RevenueDataRangeType.Week
        }).ToList();
        var monthlyReportResponses = totalRevenueByMonth.Select(x => new RevenueResponse()
        {
            Name = x.Date.ToString(),
            Value = (int)x.Total,
            IsCurrent = x.IsCurrent,
            RevenueDataRangeType = RevenueDataRangeType.Month
        }).ToList();
        return Result.Success(request.RangeType == RevenueDataRangeType.Month
            ? monthlyReportResponses
            : weeklyReportResponses);
    }
}