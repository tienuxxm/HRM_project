using Domain.Notifications;

namespace Infrastructure.Repositories;

internal sealed class NotificationRepository : Repository<Notification, NotificationId>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}