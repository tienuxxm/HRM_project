using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.FeeServices.Response;
using Application.News.GetOne;
using Domain.Abstractions;
using Domain.FreeServices;
using Domain.News;

namespace Application.FeeServices.GetOne;

public class GetOneFeeServicesCommandHandler : ICommandHandler<GetOneFeeServicesCommand, FeeAndServiceResponse>
{
    private readonly IFeeServiceRepository _feeServiceRepository;

    public GetOneFeeServicesCommandHandler(IFeeServiceRepository feeServiceRepository)
    {
        _feeServiceRepository = feeServiceRepository;
    }

    public async Task<Result<FeeAndServiceResponse>> Handle(GetOneFeeServicesCommand request,
        CancellationToken cancellationToken)
    {
        var feeService = await _feeServiceRepository.GetByIdAsync(request.FeeServiceId);
        if (feeService is null)
            return Result.Failure<FeeAndServiceResponse>(FeeServicesError.NotFound);

        var feeServiceResponse = new FeeAndServiceResponse()
        {
            Id = feeService.Id.Value,
            FeeAndServiceName = feeService.FeeName.Value,
            FeeType = feeService.FeeType,
            FeeAmount = feeService.FeeAmount,
            FeePercent = feeService.FeePercent,
            IsPercent = feeService.IsPercent,
            IsActive = feeService.IsActive
        };
        return Result.Success(feeServiceResponse);
    }
}