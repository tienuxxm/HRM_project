namespace Domain.LeaveApproverAssignments;

public record LeaveApproverAssignmentId(Guid Value)
{
    public static LeaveApproverAssignmentId New() => new(Guid.NewGuid());
}
