using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.FreeServices;
using Domain.News;
using Domain.Shared;

namespace Application.FeeServices.Update;

public class UpdateFeeServicesCommandHandler : ICommandHandler<UpdateFeeServicesCommand>
{
    private readonly IFeeServiceRepository _feeServiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFeeServicesCommandHandler(IFeeServiceRepository feeServiceRepository,
        IUnitOfWork unitOfWork)
    {
        _feeServiceRepository = feeServiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateFeeServicesCommand body, CancellationToken cancellationToken)
    {
        var feeServices = await _feeServiceRepository.GetByIdAsync(new FeeServiceId(body.id), cancellationToken);
        if (feeServices is null)
            return Result.Failure(NewsError.NotFound);
        feeServices.Update(
            new FeeName(body.FeeName), 
            body.IsPercent,
            body.FeeAmount.HasValue ? new Money(body.FeeAmount.Value, Currency.Vnd) : null,
            body.FeePercent,
            body.FeeType,
            body.IsActive
            );
        _feeServiceRepository.Update(feeServices);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}