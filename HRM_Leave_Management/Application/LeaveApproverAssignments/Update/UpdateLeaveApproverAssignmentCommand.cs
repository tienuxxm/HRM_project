using Application.Abstractions.Messaging;
using Application.Response;

namespace Application.LeaveApproverAssignments.Update;

public sealed record UpdateLeaveApproverAssignmentCommand(
    Guid Id,
    Guid ApproverEmployeeId,
    Guid? TargetDepartmentId,
    Guid? TargetPositionId,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo) : ICommand<BooleanResponse>;
