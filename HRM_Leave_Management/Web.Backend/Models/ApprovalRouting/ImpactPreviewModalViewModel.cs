using Application.ApprovalRouting.Queries.GetEligibleManualApprovers;
using Application.ApprovalRouting.Queries.GetLevelAssignmentUnassignImpact;

namespace Web.Backend.Models.ApprovalRouting;

public class ImpactPreviewModalViewModel
{
    public Guid TargetLevelAssignmentId { get; set; }
    public Guid TargetEmployeeId { get; set; }
    public string TargetSlotName { get; set; } = default!;
    public string AssignedEmployeeName { get; set; } = default!;
    public LevelAssignmentUnassignImpactResponse ImpactData { get; set; } = null!;
    public List<EligibleApproverDto> AvailableApprovers { get; set; } = new();
}
