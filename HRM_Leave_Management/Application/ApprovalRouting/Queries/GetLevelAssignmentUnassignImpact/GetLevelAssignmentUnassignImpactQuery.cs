using Application.Abstractions.Messaging;

namespace Application.ApprovalRouting.Queries.GetLevelAssignmentUnassignImpact;

public sealed record AffectedUnassignRequestDto(
    Guid LeaveRequestId,
    string RequesterName,
    string LeaveTypeName,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Duration,
    bool IsAutoReroutable,
    string ProposedApproverName,
    string ResolverStatus);

public sealed record LevelAssignmentUnassignImpactResponse(
    Guid LevelAssignmentId,
    Guid TargetEmployeeId,
    string LevelName,
    string AssignedEmployeeName,
    int TotalPendingRequestsCount,
    int AutoReroutableCount,
    int NeedsAdminAttentionCount,
    List<AffectedUnassignRequestDto> AffectedRequests);

public sealed record GetLevelAssignmentUnassignImpactQuery(Guid LevelAssignmentId)
    : IQuery<LevelAssignmentUnassignImpactResponse>;
