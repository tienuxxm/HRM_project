using Application.Abstractions.Messaging;
using Application.Response;

namespace Application.LeaveTypes.Create;

public sealed record CreateLeaveTypeCommand(
    string Name,
    string Code,
    int DefaultDays,
    string? Description) : ICommand<BooleanResponse>;
