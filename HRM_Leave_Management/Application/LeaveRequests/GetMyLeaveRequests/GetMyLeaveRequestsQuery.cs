using Application.Abstractions.Messaging;

namespace Application.LeaveRequests.GetMyLeaveRequests;

/// <summary>
/// Dashboard W1: Retrieves the most recent leave requests created by the current logged-in user.
/// Read-only query. No parameters needed — scoped internally by IUserContext.
/// </summary>
public sealed record GetMyLeaveRequestsQuery() : IQuery<List<MyLeaveRequestItem>>;

public sealed class MyLeaveRequestItem
{
    public Guid Id { get; init; }
    public string LeaveTypeName { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public decimal Duration { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
