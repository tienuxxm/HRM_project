using Application.Abstractions.Messaging;
using Application.FeeServices.Response;
using Domain.Abstractions;
using Domain.FreeServices;

namespace Application.FeeServices.GetAllPaged;

public class
    GetAllFeeAndServicePagedCommandHandler : ICommandHandler<GetAllFeeAndServicePagedCommand,
        PagedList<FeeAndServiceResponse>>
{
    private readonly IFeeServiceRepository _feeServiceRepository;

    public GetAllFeeAndServicePagedCommandHandler(IFeeServiceRepository feeServiceRepository)
    {
        _feeServiceRepository = feeServiceRepository;
    }

    public async Task<Result<PagedList<FeeAndServiceResponse>>> Handle(GetAllFeeAndServicePagedCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _feeServiceRepository.GetAllPaged(request);
        var resultDto = result.Data.Select(x =>
        {
            var resultValue = x.IsPercent ? x.FeePercent + "%" : x.FeeAmount?.Amount + " " + x.FeeAmount?.Currency.Code;
            return new FeeAndServiceResponse()
            {
                FeeAndServiceName = x.FeeName.Value,
                FeeAndServiceValue = resultValue,
                Id = x.Id.Value,
                FeeType = x.FeeType,
                IsActive = x.IsActive
            };
        }).ToList();
        return Result.Success(new PagedList<FeeAndServiceResponse>(resultDto, result.TotalCount, result.CurrentPage,
            result.PageSize));
    }
}