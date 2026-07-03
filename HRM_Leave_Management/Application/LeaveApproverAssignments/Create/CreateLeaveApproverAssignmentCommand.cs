using Application.Abstractions.Messaging;
using Application.Response;

namespace Application.LeaveApproverAssignments.Create;

public sealed record CreateLeaveApproverAssignmentCommand(
    Guid ApproverEmployeeId,
    Guid? TargetDepartmentId,
    Guid? TargetPositionId,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo) : ICommand<BooleanResponse>;
