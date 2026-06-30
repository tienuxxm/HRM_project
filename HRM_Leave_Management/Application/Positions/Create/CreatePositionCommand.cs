using Application.Abstractions.Messaging;
using Application.Response;

namespace Application.Positions.Create;

public sealed record CreatePositionCommand(
    string Code,
    string Name,
    int Level) : ICommand<BooleanResponse>;
