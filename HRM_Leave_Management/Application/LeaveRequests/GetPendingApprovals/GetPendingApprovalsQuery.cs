using Application.Abstractions.Messaging;

namespace Application.LeaveRequests.GetPendingApprovals;

/// <summary>
/// Dashboard W4: Retrieves pending leave requests awaiting approval by the current user.
/// Dynamic Approval Routing Scope rules (Phase 7):
///   - CanViewAllApprovals=true (Admin/HR via UPDATE_LEAVE_APPROVER_ASSIGNMENT): all pending requests with active dynamic assignment (AssignmentStatus == Assigned).
///   - CanViewAllApprovals=false: scoped by LeaveRequestApprovalAssignment (AssignmentStatus == Assigned), exclude own.
/// Max 4 items ordered by oldest first (FIFO).
/// Read-only query. Scoped internally by IUserContext.
/// </summary>
public sealed record GetPendingApprovalsQuery(bool CanViewAllApprovals = false) : IQuery<List<PendingApprovalItem>>;

public sealed class PendingApprovalItem
{
    public Guid Id { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string EmployeeCode { get; init; } = string.Empty;
    public string LeaveTypeName { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public decimal Duration { get; init; }
    public DateTime CreatedAt { get; init; }
    /// <summary>Number of days this request has been pending (from CreatedAt to now).</summary>
    public int PendingDays { get; init; }
}
