using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.FreeServices;
using Domain.Shared;

namespace Application.FeeServices.Create;

public class CreateFeeServiceCommandHandler : ICommandHandler<CreateFeeServiceCommand>
{
    private readonly IFeeServiceRepository _feeServiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFeeServiceCommandHandler(IFeeServiceRepository feeServiceRepository, IUnitOfWork unitOfWork)
    {
        _feeServiceRepository = feeServiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CreateFeeServiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var feeServices = request.FeeServiceRequests.Select(x => FeeService.Create(new FeeName(x.FeeName),
                x.IsPercent,
                x.FeeAmount != null ? new Money(x.FeeAmount.Value, Currency.Vnd) : null, x.FeePercent, x.FeeType,
                x.IsActive)).ToList();
            _feeServiceRepository.AddRange(feeServices);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(new Error("Create.FeeService.Fail", "Fail to create fee service"));
        }
    }
}