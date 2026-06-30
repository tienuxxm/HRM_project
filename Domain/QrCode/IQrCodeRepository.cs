namespace Domain.QrCode;

public interface IQrCodeRepository
{
    void Add(QrCode qrCode);
    IQueryable<QrCode> GetEntitiesAsQueryable();
}