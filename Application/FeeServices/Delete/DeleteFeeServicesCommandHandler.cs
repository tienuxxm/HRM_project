using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.FreeServices;
using Domain.News;

namespace Application.FeeServices.Delete;

internal sealed class DeleteFeeServicesCommandHandler : ICommandHandler<DeleteFeeServicesCommand>
{
    private readonly IFeeServiceRepository _feeServiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFeeServicesCommandHandler(IFeeServiceRepository feeServiceRepository, IUnitOfWork unitOfWork)
    {
        _feeServiceRepository = feeServiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteFeeServicesCommand request, CancellationToken cancellationToken)
    {
        var feeService = await _feeServiceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (feeService is null)
            return Result.Failure(NewsError.NotFound);
        _feeServiceRepository.Remove(feeService);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}