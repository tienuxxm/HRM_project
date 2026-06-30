using Application.Abstractions.Messaging;
using Application.Response;

namespace Application.LeaveRequests.Cancel;

public sealed record CancelLeaveRequestCommand(Guid LeaveRequestId) : ICommand<BooleanResponse>;
