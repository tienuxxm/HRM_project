using Application.Abstractions.Messaging;
using Application.Response;

namespace Application.Positions.Delete;

public sealed record DeletePositionCommand : ICommand<BooleanResponse>
{
    public Guid Id { get; init; }
}
