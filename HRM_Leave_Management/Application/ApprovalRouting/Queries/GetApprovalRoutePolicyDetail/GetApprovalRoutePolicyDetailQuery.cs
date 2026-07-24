using Application.Abstractions.Messaging;

namespace Application.ApprovalRouting.Queries.GetApprovalRoutePolicyDetail;

public record LevelSlotSummaryDto(
    Guid LevelId,
    string LevelName,
    int LevelRank,
    bool CanApproveLeave,
    Guid? CurrentlyAssignedEmployeeId,
    string? CurrentlyAssignedEmployeeName,
    string? CurrentlyAssignedEmployeeCode,
    bool IsAssigned);

public record RuleCandidateSummaryDto(
    Guid CandidateId,
    Guid LevelId,
    string LevelName,
    int LevelRank,
    int PriorityOrder,
    string? AssignedApproverName);

public record PositionRuleSummaryDto(
    Guid RuleId,
    Guid RequesterPositionId,
    string RequesterPositionName,
    bool IsSpecificApproverOverride,
    Guid? SpecificApproverEmployeeId,
    string? SpecificApproverName,
    string? SpecificApproverCode,
    bool IsActive,
    List<RuleCandidateSummaryDto> Candidates);

public record ApprovalRoutePolicyDetailDto(
    Guid PolicyId,
    string PolicyName,
    Guid DepartmentId,
    string DepartmentName,
    bool IsActive,
    DateTime CreatedAt,
    List<LevelSlotSummaryDto> LevelSlots,
    List<PositionRuleSummaryDto> PositionRules);

public record GetApprovalRoutePolicyDetailQuery(Guid PolicyId) : IQuery<ApprovalRoutePolicyDetailDto>;
