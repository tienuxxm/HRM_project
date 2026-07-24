using Application.Abstractions.Messaging;

namespace Application.ApprovalRouting.Commands.UnassignApprovalLevel;

public sealed record UnassignApprovalLevelResponse(
    Guid LevelAssignmentId,
    bool Unassigned,
    DateOnly DeactivatedDate,
    string Reason);

public sealed record UnassignApprovalLevelCommand(
    Guid LevelAssignmentId,
    DateOnly? EffectiveToDate,
    Guid? NewApproverEmployeeId,
    bool AutoRerouteUsingResolver,
    string Reason) : ICommand<UnassignApprovalLevelResponse>;
