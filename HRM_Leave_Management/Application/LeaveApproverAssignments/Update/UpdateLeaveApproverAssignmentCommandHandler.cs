using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.Employees;
using Domain.Departments;
using Domain.Positions;
using Domain.LeaveApproverAssignments;

namespace Application.LeaveApproverAssignments.Update;

internal sealed class UpdateLeaveApproverAssignmentCommandHandler : ICommandHandler<UpdateLeaveApproverAssignmentCommand, BooleanResponse>
{
    private readonly ILeaveApproverAssignmentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLeaveApproverAssignmentCommandHandler(
        ILeaveApproverAssignmentRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(UpdateLeaveApproverAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignmentId = new LeaveApproverAssignmentId(request.Id);
        var assignment = await _repository.GetByIdAsync(assignmentId, cancellationToken);
        if (assignment == null)
        {
            return Result.Failure<BooleanResponse>(LeaveApproverAssignmentErrors.NotFound);
        }

        var approverId = new EmployeeId(request.ApproverEmployeeId);
        var deptId = request.TargetDepartmentId.HasValue ? new DepartmentId(request.TargetDepartmentId.Value) : null;
        var positionId = request.TargetPositionId.HasValue ? new PositionId(request.TargetPositionId.Value) : null;

        // Check duplicate assignment (excluding current assignment)
        var isDuplicate = await _repository.IsExistedAsync(x =>
            x.Id != assignmentId &&
            x.ApproverEmployeeId == approverId &&
            x.TargetDepartmentId == deptId &&
            x.TargetPositionId == positionId &&
            x.IsActive,
            cancellationToken);

        if (isDuplicate)
        {
            return Result.Failure<BooleanResponse>(LeaveApproverAssignmentErrors.DuplicateAssignment);
        }

        assignment.Update(
            approverId,
            deptId,
            positionId,
            request.EffectiveFrom,
            request.EffectiveTo);

        _repository.Update(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BooleanResponse
        {
            Result = true,
            Message = "Leave approver assignment updated successfully."
        });
    }
}
