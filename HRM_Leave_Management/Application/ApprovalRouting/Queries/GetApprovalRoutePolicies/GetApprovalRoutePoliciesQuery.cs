using Application.Abstractions.Messaging;

namespace Application.ApprovalRouting.Queries.GetApprovalRoutePolicies;

public record ApprovalRoutePolicySummaryDto(
    Guid PolicyId,
    string PolicyName,
    Guid DepartmentId,
    string DepartmentName,
    int ActiveRulesCount,
    int LevelSlotsCount,
    string LevelSlotsSummary,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastUpdatedAt);

public record GetApprovalRoutePoliciesQuery(
    string? DepartmentFilter = null,
    string? SearchTerm = null) : IQuery<List<ApprovalRoutePolicySummaryDto>>;
