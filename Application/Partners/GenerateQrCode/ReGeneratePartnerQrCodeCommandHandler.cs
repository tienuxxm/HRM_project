using Application.Abstractions.Messaging;
using Application.FileUpload;
using Domain.Abstractions;
using Domain.Partners;
using Domain.Shared;
using MediatR;
using Newtonsoft.Json;
using QRCoder;

namespace Application.Partners.GenerateQrCode;

public class ReGeneratePartnerQrCodeCommandHandler : ICommandHandler<ReGeneratePartnerQrCodeCommand>
{
    private readonly IPartnerRepository _partnerRepository;
    private readonly ISender _sender;
    private readonly IUnitOfWork _unitOfWork;

    public ReGeneratePartnerQrCodeCommandHandler(IPartnerRepository partnerRepository, ISender sender,
        IUnitOfWork unitOfWork)
    {
        _partnerRepository = partnerRepository;
        _sender = sender;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ReGeneratePartnerQrCodeCommand request, CancellationToken cancellationToken)
    {
        var partner = await _partnerRepository.GetByIdAsync(new PartnerId(request.Id), cancellationToken);
        if (partner is null)
            return Result.Failure(PartnerErrors.NotFound);
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
        _partnerRepository.Update(partner);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}