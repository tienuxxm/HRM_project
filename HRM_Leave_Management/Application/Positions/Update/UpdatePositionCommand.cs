using Application.Abstractions.Messaging;
using Application.Response;

namespace Application.Positions.Update;

public sealed record UpdatePositionCommand(
    Guid Id,
    string Code,
    string Name,
    int Level) : ICommand<BooleanResponse>;
