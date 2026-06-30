using Application.Abstractions.Messaging;
using Domain.QrCode;

namespace Application.QrCode.GetQrCode;

public record GetQrCodeCommand(QrCodeId Id) : ICommand<GetQrCodeResponse>;