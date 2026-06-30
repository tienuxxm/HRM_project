using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Positions;

namespace Application.Positions.GetAll;

internal sealed class GetAllPositionsQueryHandler : IQueryHandler<GetAllPositionsQuery, List<Position>>
{
    private readonly IPositionRepository _positionRepository;

    public GetAllPositionsQueryHandler(IPositionRepository positionRepository)
    {
        _positionRepository = positionRepository;
    }

    public async Task<Result<List<Position>>> Handle(GetAllPositionsQuery request, CancellationToken cancellationToken)
    {
        var positions = await _positionRepository.GetAll(cancellationToken);
        return positions ?? new List<Position>();
    }
}
