using Domain.MemberNotifications;

namespace Infrastructure.Repositories;

internal sealed class MemberNotificationRepository : Repository<MemberNotification, MemberNotificationId>,
    IMemberNotificationRepository
{
    public MemberNotificationRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}