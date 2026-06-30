using Domain.Abstractions;

namespace Domain.MemberNotifications;

public interface IMemberNotificationRepository
{
    void Add(MemberNotification notification);
    void AddRange(List<MemberNotification> notification);
    void Update(MemberNotification notification);
    void Remove(MemberNotification notification);
    Task<MemberNotification?> GetByIdAsync(MemberNotificationId Id, CancellationToken cancellationToken = default);

    IQueryable<MemberNotification> GetEntitiesAsQueryable();

    public Task<PagedList<MemberNotification>> GetAllPaged(PagedQuery<MemberNotification, MemberNotificationId> request,
        IQueryable<MemberNotification>? queryable = null);
}