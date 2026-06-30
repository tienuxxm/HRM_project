using Application.Abstractions.Messaging;
using Domain.LeaveTypes;

namespace Application.LeaveTypes.Update;

public record UpdateLeaveTypeCommand(
    Guid Id,
    string Name,
    string Code,
    int DefaultDays,
    string? Description) : ICommand<LeaveType>;
