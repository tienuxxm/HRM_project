using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Partners.GetOne;
using Domain.Abstractions;
using Domain.Partners;

namespace Application.Partners.GetAllPaged;

internal sealed class
    GetAllPartnerPagedCommandHandler : ICommandHandler<GetAllPartnerPagedCommand, GetAllPartnerPagedResponse>
{
    private readonly IPartnerRepository _partnerRepository;
    private readonly IAwsS3Service _awsS3Service;

    public GetAllPartnerPagedCommandHandler(
        IPartnerRepository partnerRepository, IAwsS3Service awsS3Service)
    {
        _partnerRepository = partnerRepository;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<GetAllPartnerPagedResponse>> Handle(GetAllPartnerPagedCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _partnerRepository.GetAllPaged(request);
        var partnersDto = result.Data.Select(x => new PartnerResponse
        {
            Address = x.Address?.Value,
            Email = x.Email?.Value,
            Id = x.Id.Value,
            CreatedDate = x.CreatedDate,
            PhoneNumber = x.PhoneNumber?.Value,
            PartnerName = x.PartnerName.Value,
            QrCodeId = x.QrCodeId,
            ImageUrl = x.QrCode != null ? _awsS3Service.GetUrlPresign(x.QrCode.Value, 60) : ""
        }).ToList();
        return Result.Success(new GetAllPartnerPagedResponse(partnersDto, result.TotalPages,
            result.CurrentPage, request.PageSize));
    }
}