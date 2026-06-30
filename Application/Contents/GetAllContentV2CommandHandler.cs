using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Districts;
using Domain.Provinces;
using Microsoft.EntityFrameworkCore;

namespace Application.Contents;

public class GetAllContentV2CommandHandler : ICommandHandler<GetAllContentV2Command, ContentV2Response>
{
    private readonly IProvinceRepository _provinceRepository;
    private readonly IDistrictRepository _districtRepository;

    public GetAllContentV2CommandHandler(IProvinceRepository provinceRepository, IDistrictRepository districtRepository)
    {
        _provinceRepository = provinceRepository;
        _districtRepository = districtRepository;
    }

    public async Task<Result<ContentV2Response>> Handle(GetAllContentV2Command request,
        CancellationToken cancellationToken)
    {
        var provinces = await _provinceRepository.GetEntitiesAsQueryable().Select(x => new ProvinceResponseV2()
        {
            Id = x.Id.Value,
            Name = x.Name
        }).ToListAsync(cancellationToken);

        var districts = await _districtRepository.GetEntitiesAsQueryable().Select(x => new DistrictResponse()
        {
            Id = x.Id.Value,
            Name = x.Name,
            ProvinceId = x.ProvinceId.Value
        }).ToListAsync(cancellationToken);

        return Result.Success(new ContentV2Response()
        {
            Provinces = provinces,
            Districts = districts
        });
    }
}