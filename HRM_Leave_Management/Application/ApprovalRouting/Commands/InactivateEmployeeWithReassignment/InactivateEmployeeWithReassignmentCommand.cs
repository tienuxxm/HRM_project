using Application.Abstractions.Messaging;
using Domain.Abstractions;

namespace Application.ApprovalRouting.Commands.InactivateEmployeeWithReassignment;

public sealed record InactivateEmployeeWithReassignmentResponse(
    Guid EmployeeId,
    bool EmployeeInactivated,
    int LevelSlotsUnassigned,
    int PendingRequestsProcessed,
    int ReassignedCount,
    int NeedsAdminAttentionCount);

public sealed record InactivateEmployeeWithReassignmentCommand(
    Guid EmployeeId,
    Guid? NewApproverEmployeeId,
    bool AutoReroutePendingRequests,
    string InactivateReason) : ICommand<InactivateEmployeeWithReassignmentResponse>;
