using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.Employees;
using Domain.Departments;
using Domain.Positions;
using Domain.LeaveApproverAssignments;

namespace Application.LeaveApproverAssignments.Create;

internal sealed class CreateLeaveApproverAssignmentCommandHandler : ICommandHandler<CreateLeaveApproverAssignmentCommand, BooleanResponse>
{
    private readonly ILeaveApproverAssignmentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLeaveApproverAssignmentCommandHandler(
        ILeaveApproverAssignmentRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(CreateLeaveApproverAssignmentCommand request, CancellationToken cancellationToken)
    {
        var approverId = new EmployeeId(request.ApproverEmployeeId);
        var deptId = request.TargetDepartmentId.HasValue ? new DepartmentId(request.TargetDepartmentId.Value) : null;
        var positionId = request.TargetPositionId.HasValue ? new PositionId(request.TargetPositionId.Value) : null;

        // Check duplicate assignment
        var isDuplicate = await _repository.IsExistedAsync(x =>
            x.ApproverEmployeeId == approverId &&
            x.TargetDepartmentId == deptId &&
            x.TargetPositionId == positionId &&
            x.IsActive,
            cancellationToken);

        if (isDuplicate)
        {
            return Result.Failure<BooleanResponse>(LeaveApproverAssignmentErrors.DuplicateAssignment);
        }

        var assignment = LeaveApproverAssignment.Create(
            approverId,
            deptId,
            positionId,
            request.EffectiveFrom,
            request.EffectiveTo);

        _repository.Add(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BooleanResponse
        {
            Result = true,
            Message = "Leave approver assignment created successfully."
        });
    }
}
