using Application.Abstractions.Messaging;
using Application.Partners.GetOne;
using Domain.Abstractions;
using Domain.Partners;

namespace Application.Partners.GetAll;

internal sealed class GetAllPartnerCommandHandler : ICommandHandler<GetAllPartnerCommand, List<PartnerResponse>>
{
    private readonly IPartnerRepository _partnerRepository;

    public GetAllPartnerCommandHandler(IPartnerRepository partnerRepository)
    {
        _partnerRepository = partnerRepository;
    }

    public async Task<Result<List<PartnerResponse>>> Handle(GetAllPartnerCommand request,
        CancellationToken cancellationToken)
    {
        var partners = await _partnerRepository.GetAll(cancellationToken);
        if (partners is null)
            return Result.Success(new List<PartnerResponse>());
        var partnersDto = partners.Select(p => new PartnerResponse()
        {
            Id = p.Id.Value,
            CreatedDate = p.CreatedDate,
            PartnerName = p.PartnerName.Value
        }).ToList();
        return Result.Success(partnersDto);
    }
}