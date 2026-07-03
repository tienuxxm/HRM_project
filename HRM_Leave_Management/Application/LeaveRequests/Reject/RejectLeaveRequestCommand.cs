using Application.Abstractions.Messaging;
using Application.Response;

namespace Application.LeaveRequests.Reject;

public sealed record RejectLeaveRequestCommand(
    Guid LeaveRequestId,
    string? Comment) : ICommand<BooleanResponse>;
