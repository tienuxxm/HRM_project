using Application.ApprovalRouting.Queries.GetLevelSlotAssignments;

namespace Web.Backend.Models.ApprovalRouting;

public class LevelAssignmentViewModel
{
    public LevelSlotAssignmentsDto Data { get; set; } = null!;
    public bool CanUpdate { get; set; }
}
