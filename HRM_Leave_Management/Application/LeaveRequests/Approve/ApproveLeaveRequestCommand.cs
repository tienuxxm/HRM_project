using Application.Abstractions.Messaging;
using Application.Response;

namespace Application.LeaveRequests.Approve;

public sealed record ApproveLeaveRequestCommand(
    Guid LeaveRequestId,
    string? Comment) : ICommand<BooleanResponse>;
