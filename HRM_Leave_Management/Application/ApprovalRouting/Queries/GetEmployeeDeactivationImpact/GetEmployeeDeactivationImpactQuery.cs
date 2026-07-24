using Application.Abstractions.Messaging;
using Domain.Abstractions;

namespace Application.ApprovalRouting.Queries.GetEmployeeDeactivationImpact;

public sealed record AffectedPendingLeaveRequestDto(
    Guid LeaveRequestId,
    string RequesterName,
    string LeaveTypeName,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Duration,
    string Status);

public sealed record AffectedLevelSlotDto(
    Guid LevelAssignmentId,
    Guid LevelId,
    string LevelName,
    string DepartmentName,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo);

public sealed record AffectedSpecificRuleDto(
    Guid RuleId,
    string DepartmentName,
    string RequesterPositionName);

public sealed record EmployeeDeactivationImpactResponse(
    Guid EmployeeId,
    string EmployeeName,
    int PendingLeaveRequestsCount,
    List<AffectedPendingLeaveRequestDto> AffectedPendingRequests,
    int AssignedLevelSlotsCount,
    List<AffectedLevelSlotDto> AffectedLevelSlots,
    int SpecificApproverRulesCount,
    List<AffectedSpecificRuleDto> AffectedSpecificRules);

public sealed record GetEmployeeDeactivationImpactQuery(Guid EmployeeId)
    : IQuery<EmployeeDeactivationImpactResponse>;
