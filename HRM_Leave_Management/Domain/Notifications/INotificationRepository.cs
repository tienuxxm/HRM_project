using Domain.Abstractions;

namespace Domain.Notifications;

public interface INotificationRepository
{
    void Add(Notification notification);
    void AddRange(List<Notification> notifications);

    Task<PagedList<Notification>> GetAllPaged(PagedQuery<Notification, NotificationId> request,
        IQueryable<Notification>? queryable = null);

    IQueryable<Notification> GetEntitiesAsQueryable();

    Task<Notification?> GetByIdAsync(
        NotificationId id,
        CancellationToken cancellationToken = default);
}