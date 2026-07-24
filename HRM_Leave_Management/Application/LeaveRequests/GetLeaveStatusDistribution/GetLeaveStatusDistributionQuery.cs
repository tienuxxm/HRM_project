using Application.Abstractions.Messaging;

namespace Application.LeaveRequests.GetLeaveStatusDistribution;

/// <summary>
/// Dashboard W2: Retrieves leave request status distribution counts.
/// Scope is permission-aware: personal for employees, management-scope for approvers/admin.
/// Read-only query.
/// </summary>
public sealed record GetLeaveStatusDistributionQuery() : IQuery<LeaveStatusDistributionResult>;

public sealed class LeaveStatusDistributionResult
{
    public int ApprovedCount { get; init; }
    public int PendingCount { get; init; }
    public int RejectedCount { get; init; }
    public int CanceledCount { get; init; }
    public int TotalCount => ApprovedCount + PendingCount + RejectedCount + CanceledCount;
}
