using Application.Abstractions.Messaging;

namespace Application.ApprovalRouting.Queries.GetEligibleManualApprovers;

public sealed record EligibleApproverDto(
    Guid EmployeeId,
    string EmployeeCode,
    string FullName,
    string PositionName,
    string DepartmentName);

public sealed record GetEligibleManualApproversQuery(
    Guid TargetEmployeeId,
    Guid? TargetLevelAssignmentId = null)
    : IQuery<List<EligibleApproverDto>>;
