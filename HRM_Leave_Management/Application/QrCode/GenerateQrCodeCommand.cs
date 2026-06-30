using Application.Abstractions.Messaging;

namespace Application.QrCode;

public record GenerateQrCodeCommand(string Type, string Id, string Code) : ICommand<string>;