namespace Domain.ApprovalRouting;

public enum ApprovalAssignmentReason
{
    DirectLevelMatch = 1,
    SuperiorLevelEscalated = 2,
    SpecificEmployeeOverride = 3,
    OperatorManualReassigned = 4,
    AutoApproved = 5
}
