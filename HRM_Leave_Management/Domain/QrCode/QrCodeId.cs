namespace Domain.QrCode;

public record QrCodeId(Guid Value)
{
    public static QrCodeId New => new QrCodeId(Guid.NewGuid());
}