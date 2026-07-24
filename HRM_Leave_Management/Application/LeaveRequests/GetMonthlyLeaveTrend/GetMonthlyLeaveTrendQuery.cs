using Application.Abstractions.Messaging;

namespace Application.LeaveRequests.GetMonthlyLeaveTrend;

/// <summary>
/// Dashboard W3: Retrieves monthly leave request counts for the past 6 months.
/// Scope is permission-aware (same rules as W2).
/// Read-only query.
/// </summary>
public sealed record GetMonthlyLeaveTrendQuery() : IQuery<List<MonthlyLeaveTrendItem>>;

public sealed class MonthlyLeaveTrendItem
{
    public string MonthLabel { get; init; } = string.Empty;
    public int Year { get; init; }
    public int Month { get; init; }
    public int RequestCount { get; init; }
}
