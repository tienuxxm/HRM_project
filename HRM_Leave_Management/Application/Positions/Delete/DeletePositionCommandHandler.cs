using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.Employees;
using Domain.Positions;

namespace Application.Positions.Delete;

internal sealed class DeletePositionCommandHandler : ICommandHandler<DeletePositionCommand, BooleanResponse>
{
    private readonly IPositionRepository _positionRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePositionCommandHandler(
        IPositionRepository positionRepository,
        IEmployeeRepository employeeRepository,
        IUnitOfWork unitOfWork)
    {
        _positionRepository = positionRepository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(DeletePositionCommand request, CancellationToken cancellationToken)
    {
        var positionId = new PositionId(request.Id);
        var position = await _positionRepository.GetByIdAsync(positionId, cancellationToken);
        if (position is null)
        {
            return Result.Failure<BooleanResponse>(PositionErrors.NotFound);
        }

        // Check if there are any employees assigned to this position
        var hasEmployees = await _employeeRepository.IsExistedAsync(
            x => x.PositionId == positionId, 
            cancellationToken);
        if (hasEmployees)
        {
            return Result.Failure<BooleanResponse>(PositionErrors.HasEmployees);
        }

        position.SetActive(false);
        _positionRepository.Update(position);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BooleanResponse
        {
            Result = true,
            Message = $"Position {position.Name} has been deleted successfully."
        });
    }
}
