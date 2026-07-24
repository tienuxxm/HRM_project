using Application.Abstractions.Messaging;

namespace Application.LeaveRequests.GetApprovalAging;

/// <summary>
/// Dashboard W5: Retrieves aging summary of pending approvals in the current user's scope.
/// Dynamic Approval Routing Scope rules (Phase 7):
///   - CanViewAllApprovals=true (Admin/HR via UPDATE_LEAVE_APPROVER_ASSIGNMENT): all pending requests with active dynamic assignment (AssignmentStatus == Assigned).
///   - CanViewAllApprovals=false: scoped by LeaveRequestApprovalAssignment (AssignmentStatus == Assigned), exclude own.
/// Aging buckets: Today (0 days), 1-2 days, 3+ days (overdue).
/// Read-only query. Scoped internally by IUserContext.
/// </summary>
public sealed record GetApprovalAgingQuery(bool CanViewAllApprovals = false) : IQuery<ApprovalAgingResult>;

public sealed class ApprovalAgingResult
{
    /// <summary>Pending requests submitted today (age = 0 days).</summary>
    public int TodayCount { get; init; }
    /// <summary>Pending requests submitted 1-2 days ago.</summary>
    public int OneToTwoDaysCount { get; init; }
    /// <summary>Pending requests submitted 3+ days ago (overdue).</summary>
    public int OverdueCount { get; init; }
    public int TotalPending => TodayCount + OneToTwoDaysCount + OverdueCount;

    /// <summary>Top 3 oldest overdue requests for urgent attention display.</summary>
    public List<OverdueItem> TopOverdue { get; init; } = new();
}

public sealed class OverdueItem
{
    public Guid Id { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string LeaveTypeName { get; init; } = string.Empty;
    public int AgeDays { get; init; }
    public DateTime CreatedAt { get; init; }
}
