using Application.Abstractions.Messaging;

namespace Application.LeaveBalances.GetMyLeaveBalance;

/// <summary>
/// Dashboard W6: Retrieves the current logged-in employee's Annual Leave balance for the current year.
/// Read-only query. No parameters needed — scoped internally by IUserContext.
/// </summary>
public sealed record GetMyLeaveBalanceQuery() : IQuery<MyLeaveBalanceResult?>;

public sealed class MyLeaveBalanceResult
{
    public decimal AllocatedDays { get; init; }
    public decimal UsedDays { get; init; }
    public decimal PendingDays { get; init; }
    public decimal AvailableDays => AllocatedDays - UsedDays - PendingDays;
    public string LeaveTypeName { get; init; } = string.Empty;
    public int Year { get; init; }
}
