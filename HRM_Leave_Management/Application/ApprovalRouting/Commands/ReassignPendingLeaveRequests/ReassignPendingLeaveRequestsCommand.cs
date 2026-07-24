using Application.Abstractions.Messaging;
using Domain.Abstractions;

namespace Application.ApprovalRouting.Commands.ReassignPendingLeaveRequests;

public sealed record ReassignPendingLeaveRequestsResponse(
    int TotalProcessed,
    int ReassignedCount,
    int NeedsAdminAttentionCount);

public sealed record ReassignPendingLeaveRequestsCommand(
    Guid TargetEmployeeId,
    Guid? NewApproverEmployeeId,
    bool AutoRerouteUsingResolver,
    string Reason,
    Guid? TargetLevelAssignmentId = null) : ICommand<ReassignPendingLeaveRequestsResponse>;
