namespace Domain.ApprovalRouting;

public record ApprovalRoutePolicyId(Guid Value)
{
    public static ApprovalRoutePolicyId New() => new(Guid.NewGuid());
}

public record ApprovalRouteLevelId(Guid Value)
{
    public static ApprovalRouteLevelId New() => new(Guid.NewGuid());
}

public record ApprovalRouteLevelAssignmentId(Guid Value)
{
    public static ApprovalRouteLevelAssignmentId New() => new(Guid.NewGuid());
}

public record ApprovalRouteRuleId(Guid Value)
{
    public static ApprovalRouteRuleId New() => new(Guid.NewGuid());
}

public record ApprovalRouteRuleCandidateId(Guid Value)
{
    public static ApprovalRouteRuleCandidateId New() => new(Guid.NewGuid());
}

public record LeaveRequestApprovalAssignmentId(Guid Value)
{
    public static LeaveRequestApprovalAssignmentId New() => new(Guid.NewGuid());
}

public record ApprovalRouteAuditLogId(Guid Value)
{
    public static ApprovalRouteAuditLogId New() => new(Guid.NewGuid());
}
