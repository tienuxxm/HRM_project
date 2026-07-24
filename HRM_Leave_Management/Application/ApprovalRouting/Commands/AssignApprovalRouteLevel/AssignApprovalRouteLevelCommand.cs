using Application.Abstractions.Messaging;

namespace Application.ApprovalRouting.Commands.AssignApprovalRouteLevel;

public sealed record AssignApprovalRouteLevelCommand(
    Guid LevelId,
    Guid ApproverEmployeeId,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo,
    string? Reason) : ICommand<Guid>;
