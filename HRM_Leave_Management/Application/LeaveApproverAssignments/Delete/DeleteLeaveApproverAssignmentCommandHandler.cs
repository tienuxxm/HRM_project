using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.LeaveApproverAssignments;

namespace Application.LeaveApproverAssignments.Delete;

internal sealed class DeleteLeaveApproverAssignmentCommandHandler : ICommandHandler<DeleteLeaveApproverAssignmentCommand, BooleanResponse>
{
    private readonly ILeaveApproverAssignmentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteLeaveApproverAssignmentCommandHandler(
        ILeaveApproverAssignmentRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(DeleteLeaveApproverAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignmentId = new LeaveApproverAssignmentId(request.Id);
        var assignment = await _repository.GetByIdAsync(assignmentId, cancellationToken);
        if (assignment == null)
        {
            return Result.Failure<BooleanResponse>(LeaveApproverAssignmentErrors.NotFound);
        }

        assignment.SetActive(false);
        _repository.Update(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BooleanResponse
        {
            Result = true,
            Message = "Leave approver assignment has been deleted successfully."
        });
    }
}
