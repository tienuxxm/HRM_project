using Application.Abstractions.Messaging;
using Application.ApprovalRouting.Services;
using Domain.Abstractions;

namespace Application.ApprovalRouting.Commands.ReassignPendingLeaveRequests;

internal sealed class ReassignPendingLeaveRequestsCommandHandler
    : ICommandHandler<ReassignPendingLeaveRequestsCommand, ReassignPendingLeaveRequestsResponse>
{
    private readonly IApprovalReassignmentService _reassignmentService;
    private readonly IUnitOfWork _unitOfWork;

    public ReassignPendingLeaveRequestsCommandHandler(
        IApprovalReassignmentService reassignmentService,
        IUnitOfWork unitOfWork)
    {
        _reassignmentService = reassignmentService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReassignPendingLeaveRequestsResponse>> Handle(
        ReassignPendingLeaveRequestsCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _reassignmentService.ExecuteReassignmentAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return result;
        }

        // Standalone commit owner for ReassignPendingLeaveRequestsCommand
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }
}
