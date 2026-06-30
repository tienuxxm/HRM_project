using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Application.FileUpload;
using Domain.Abstractions;
using Domain.Partners;
using Domain.QrCode;
using Domain.Shared;
using MediatR;
using Newtonsoft.Json;
using QRCoder;

namespace Application.Partners.Create;

internal class CreatePartnerCommandHandler : ICommandHandler<CreatePartnerCommand, Guid>
{
    private readonly IPartnerRepository _partnerRepository;
    private readonly IQrCodeRepository _qrCodeRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISender _sender;

    public CreatePartnerCommandHandler(
        IPartnerRepository partnerRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork, ISender sender, IQrCodeRepository qrCodeRepository)
    {
        _partnerRepository = partnerRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
        _sender = sender;
        _qrCodeRepository = qrCodeRepository;
    }

    public async Task<Result<Guid>> Handle(CreatePartnerCommand request, CancellationToken cancellationToken)
    {
        var partner = Partner.Create(request.Name, request.Address, request.PhoneNumber, request.Email,
            _dateTimeProvider.UtcNow);
        var imageKey = Guid.NewGuid();
        var qrCodeContent = new { Type = "PARTNER", Id = partner?.Id.Value.ToString() };
        var jsonQrCodeContent = JsonConvert.SerializeObject(qrCodeContent);
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(jsonQrCodeContent, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new BitmapByteQRCode(qrCodeData);
        var qcCodeBytes = qrCode.GetGraphic(20);
        using var stream = new MemoryStream(qcCodeBytes);
        var fileUploadCommand = new FileUploadCommand(stream, imageKey.ToString());
        await _sender.Send(fileUploadCommand, cancellationToken);
        partner?.SetQrCodeImage(new ImageUrl(imageKey.ToString()));
        if (partner is not null)
        {
            var qrCodeId = Domain.QrCode.QrCode.Create(new QrCodeLinkId(partner.Id.Value), QrCodeType.PARTNER);
            _qrCodeRepository.Add(qrCodeId);
            partner.SetQrCodeId(qrCodeId.Id.Value.ToString());
        }

        _partnerRepository.Add(partner);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return partner.Id.Value;
    }
}