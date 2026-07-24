using Application.Abstractions.Messaging;

namespace Application.ApprovalRouting.Commands.AddApprovalRouteRule;

public sealed record AddApprovalRouteRuleCommand(
    Guid PolicyId,
    Guid RequesterPositionId,
    Guid CandidateLevelId,
    int PriorityOrder) : ICommand<Guid>;
