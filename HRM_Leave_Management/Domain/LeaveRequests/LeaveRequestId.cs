namespace Domain.LeaveRequests;

public record LeaveRequestId(Guid Value)
{
    public static LeaveRequestId New() => new(Guid.NewGuid());
}
