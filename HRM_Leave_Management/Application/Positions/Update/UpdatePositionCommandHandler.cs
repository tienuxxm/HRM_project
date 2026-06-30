using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.Positions;

namespace Application.Positions.Update;

internal sealed class UpdatePositionCommandHandler : ICommandHandler<UpdatePositionCommand, BooleanResponse>
{
    private readonly IPositionRepository _positionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePositionCommandHandler(IPositionRepository positionRepository, IUnitOfWork unitOfWork)
    {
        _positionRepository = positionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(UpdatePositionCommand request, CancellationToken cancellationToken)
    {
        var position = await _positionRepository.GetByIdAsync(new PositionId(request.Id), cancellationToken);
        if (position is null)
        {
            return Result.Failure<BooleanResponse>(PositionErrors.NotFound);
        }

        if (position.Code != request.Code)
        {
            var isPositionExisted = await _positionRepository.IsExistedAsync(x => x.Code == request.Code, cancellationToken);
            if (isPositionExisted)
            {
                return Result.Failure<BooleanResponse>(PositionErrors.PositionExisted);
            }
        }

        position.Update(
            request.Code,
            request.Name,
            request.Level);

        _positionRepository.Update(position);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BooleanResponse
        {
            Result = true,
            Message = $"Position {request.Name} ({request.Code}) has been updated successfully."
        });
    }
}
