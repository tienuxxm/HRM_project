using Application.Abstractions.Messaging;
using Application.FileUpload;
using Domain.Abstractions;
using MediatR;
using Newtonsoft.Json;
using QRCoder;

namespace Application.QrCode;

public class GenerateQrCodeCommandHandler : ICommandHandler<GenerateQrCodeCommand, string>
{
    private readonly ISender _sender;

    public GenerateQrCodeCommandHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task<Result<string>> Handle(GenerateQrCodeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var imageKey = Guid.NewGuid();
            var qrCodeContent = new { request.Type, request.Id, request.Code };
            var jsonQrCodeContent = JsonConvert.SerializeObject(qrCodeContent);
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(jsonQrCodeContent, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new BitmapByteQRCode(qrCodeData);
            var qcCodeBytes = qrCode.GetGraphic(20);
            using var stream = new MemoryStream(qcCodeBytes);
            var fileUploadCommand = new FileUploadCommand(stream, imageKey.ToString());
            var result = await _sender.Send(fileUploadCommand, cancellationToken);
            return result.IsFailure
                ? Result.Failure<string>(new Error("QrCodeGenerate.Fail", "Fail to generate qrcode"))
                : Result.Success(imageKey.ToString());
        }
        catch (Exception e)
        {
            return Result.Failure<string>(new Error("QrCodeGenerate.Fail", "Fail to generate qrcode"));
        }
    }
}