using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.Positions;

namespace Application.Positions.Create;

internal sealed class CreatePositionCommandHandler : ICommandHandler<CreatePositionCommand, BooleanResponse>
{
    private readonly IPositionRepository _positionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePositionCommandHandler(IPositionRepository positionRepository, IUnitOfWork unitOfWork)
    {
        _positionRepository = positionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
    {
        var isPositionExisted = await _positionRepository.IsExistedAsync(x => x.Code == request.Code, cancellationToken);
        if (isPositionExisted)
        {
            return Result.Failure<BooleanResponse>(PositionErrors.PositionExisted);
        }

        var position = Position.Create(
            request.Code,
            request.Name,
            request.Level);

        _positionRepository.Add(position);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BooleanResponse
        {
            Result = true,
            Message = $"Position {request.Name} ({request.Code}) has been created successfully."
        });
    }
}
