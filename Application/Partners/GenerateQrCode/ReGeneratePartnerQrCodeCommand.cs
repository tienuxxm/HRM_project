using Application.Abstractions.Messaging;

namespace Application.Partners.GenerateQrCode;

public record ReGeneratePartnerQrCodeCommand(Guid Id) : ICommand;