using Application.Abstractions.Messaging;

namespace Application.ApprovalRouting.Queries.GetLevelSlotAssignments;

public record LevelSlotAssignmentRowDto(
    Guid LevelId,
    string LevelName,
    int LevelRank,
    bool CanApproveLeave,
    Guid? AssignmentId,
    Guid? AssignedEmployeeId,
    string? AssignedEmployeeName,
    string? AssignedEmployeeCode,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsActive,
    int ImpactedPendingRequestsCount);

public record LevelSlotAssignmentsDto(
    Guid PolicyId,
    string PolicyName,
    Guid DepartmentId,
    string DepartmentName,
    List<LevelSlotAssignmentRowDto> LevelSlots);

public record GetLevelSlotAssignmentsQuery(Guid PolicyId) : IQuery<LevelSlotAssignmentsDto>;
