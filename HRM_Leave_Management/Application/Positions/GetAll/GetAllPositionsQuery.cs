using Application.Abstractions.Messaging;
using Domain.Positions;

namespace Application.Positions.GetAll;

public sealed record GetAllPositionsQuery : IQuery<List<Position>>;
