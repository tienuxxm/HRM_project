using Application.ApprovalRouting.Commands.ReassignPendingLeaveRequests;
using Domain.Abstractions;

namespace Application.ApprovalRouting.Services;

public interface IApprovalReassignmentService
{
    Task<Result<ReassignPendingLeaveRequestsResponse>> ExecuteReassignmentAsync(
        ReassignPendingLeaveRequestsCommand request,
        CancellationToken cancellationToken);
}
