using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.LeaveApproverAssignments;
using Microsoft.EntityFrameworkCore;

namespace Application.LeaveApproverAssignments.GetOne;

internal sealed class GetLeaveApproverAssignmentQueryHandler : IQueryHandler<GetLeaveApproverAssignmentQuery, LeaveApproverAssignment>
{
    private readonly ILeaveApproverAssignmentRepository _repository;

    public GetLeaveApproverAssignmentQueryHandler(ILeaveApproverAssignmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<LeaveApproverAssignment>> Handle(GetLeaveApproverAssignmentQuery request, CancellationToken cancellationToken)
    {
        var assignmentId = new LeaveApproverAssignmentId(request.Id);
        var assignment = await _repository.GetEntitiesAsQueryable()
            .Include(x => x.Approver)
            .Include(x => x.TargetDepartment)
            .Include(x => x.TargetPosition)
            .FirstOrDefaultAsync(x => x.Id == assignmentId && x.IsActive, cancellationToken);

        if (assignment == null)
        {
            return Result.Failure<LeaveApproverAssignment>(LeaveApproverAssignmentErrors.NotFound);
        }

        return Result.Success(assignment);
    }
}
