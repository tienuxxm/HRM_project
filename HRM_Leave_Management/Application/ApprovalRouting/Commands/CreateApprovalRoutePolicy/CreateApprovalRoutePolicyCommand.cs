using Application.Abstractions.Messaging;

namespace Application.ApprovalRouting.Commands.CreateApprovalRoutePolicy;

public sealed record CreateApprovalRoutePolicyCommand(
    Guid? DepartmentId,
    string Name,
    string? Description) : ICommand<Guid>;
