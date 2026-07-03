using Application.Abstractions.Messaging;
using Application.Response;

namespace Application.LeaveApproverAssignments.Delete;

public sealed record DeleteLeaveApproverAssignmentCommand(Guid Id) : ICommand<BooleanResponse>;
