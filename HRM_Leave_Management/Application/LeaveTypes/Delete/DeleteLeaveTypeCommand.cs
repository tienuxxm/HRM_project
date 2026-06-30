using Application.Abstractions.Messaging;
using Application.Response;

namespace Application.LeaveTypes.Delete;

public sealed record DeleteLeaveTypeCommand : ICommand<BooleanResponse>
{
    public required Guid Id { get; set; }
}
