using Domain.ApprovalRouting;
using Domain.Employees;

namespace Application.Abstractions.ApprovalRouting;

public interface IApprovalRouteResolverService
{
    Task<ApprovalRouteResolutionResult> ResolveApproverAsync(
        Employee requester,
        CancellationToken cancellationToken = default,
        ApprovalRouteLevelAssignmentId? excludedLevelAssignmentId = null);
}
