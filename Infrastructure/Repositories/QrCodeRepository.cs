using Domain.QrCode;

namespace Infrastructure.Repositories;

internal sealed class QrCodeRepository : Repository<QrCode, QrCodeId>, IQrCodeRepository
{
    public QrCodeRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}