using Application.Abstractions.Messaging;

namespace Application.Orders.GetRevenue;

public record GetRevenueCommand
    (RevenueDataRangeType RangeType = RevenueDataRangeType.Week) : ICommand<List<RevenueResponse>>;

public enum RevenueDataRangeType
{
    Week,
    Month,
}