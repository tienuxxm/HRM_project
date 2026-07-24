namespace Domain.ApprovalRouting;

public enum ApprovalRouteAuditActionType
{
    Created = 1,
    Reassigned = 2,
    Escalated = 3,
    NeedsAttention = 4,
    OverrideApplied = 5,
    AutoApproved = 6
}
