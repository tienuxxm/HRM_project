using Application.Abstractions.Messaging;

namespace Application.ApprovalRouting.Commands.AddApprovalRouteLevel;

public sealed record AddApprovalRouteLevelCommand(
    Guid PolicyId,
    string LevelName,
    int LevelRank,
    bool CanApproveLeave) : ICommand<Guid>;
