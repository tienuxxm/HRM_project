using Application.Abstractions.Messaging;
using Application.Response;
using Domain.LeaveRequests;

namespace Application.LeaveRequests.Create;

public sealed record CreateLeaveRequestCommand(
    Guid LeaveTypeId,
    DateOnly StartDate,
    DateOnly EndDate,
    LeaveDayPart StartDayPart,
    LeaveDayPart EndDayPart,
    string Reason) : ICommand<BooleanResponse>;
