using Domain.Abstractions;

namespace Domain.QrCode;

public class QrCode : Entity<QrCodeId>
{
    private QrCode()
    {
    }

    private QrCode(QrCodeId id, QrCodeLinkId linkId, QrCodeType type) : base(id)
    {
        LinkId = linkId;
        Type = type;
    }

    public static QrCode Create(QrCodeLinkId linkId, QrCodeType type)
    {
        return new QrCode(QrCodeId.New, linkId, type);
    }

    public QrCodeLinkId LinkId { get; private set; }
    public QrCodeType Type { get; private set; }
}