using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.LeaveApproverAssignments;
using Microsoft.EntityFrameworkCore;

namespace Application.LeaveApproverAssignments.GetAll;

internal sealed class GetAllLeaveApproverAssignmentsQueryHandler : IQueryHandler<GetAllLeaveApproverAssignmentsQuery, List<LeaveApproverAssignment>>
{
    private readonly ILeaveApproverAssignmentRepository _repository;

    public GetAllLeaveApproverAssignmentsQueryHandler(ILeaveApproverAssignmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<LeaveApproverAssignment>>> Handle(GetAllLeaveApproverAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var assignments = await _repository.GetEntitiesAsQueryable()
            .Include(x => x.Approver)
            .Include(x => x.TargetDepartment)
            .Include(x => x.TargetPosition)
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);

        return Result.Success(assignments);
    }
}
