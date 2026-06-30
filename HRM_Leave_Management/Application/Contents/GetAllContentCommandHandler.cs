using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Provinces;
using Microsoft.EntityFrameworkCore;

namespace Application.Contents;

public class GetAllContentCommandHandler : ICommandHandler<GetAllContentCommand, ContentResponse>
{
    private readonly IProvinceRepository _provinceRepository;

    public GetAllContentCommandHandler(IProvinceRepository provinceRepository)
    {
        _provinceRepository = provinceRepository;
    }

    public async Task<Result<ContentResponse>> Handle(GetAllContentCommand request, CancellationToken cancellationToken)
    {
        var data = await _provinceRepository.GetEntitiesAsQueryable()
            .Include(x => x.Districts)
            .ToListAsync(cancellationToken);
        return Result.Success(new ContentResponse()
        {
            Provinces = data.Select(x => new ProvinceResponse()
            {
                Id = x.Id.Value,
                Name = x.Name,
                Districts = x.Districts.Select(d => new DistrictResponse()
                {
                    Id = d.Id.Value,
                    Name = d.Name,
                    ProvinceId = d.ProvinceId.Value
                }).ToList()
            }).ToList()
        });
    }
}