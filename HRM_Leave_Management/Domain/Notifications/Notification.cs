using Domain.Abstractions;
using Domain.Notifications.Events;
using Domain.Shared;

namespace Domain.Notifications;

public class Notification : Entity<NotificationId>
{
    public Title Title { get; private set; }
    public DateTime CreatedDate { get; private set; }

    public Content? Content { get; private set; }

    public NotificationType NotificationType { get; private set; }

    public ReferenceId? ReferenceId { get; private set; }

    private Notification()
    {
    }

    private Notification(NotificationId id, Title title, NotificationType notificationType, ReferenceId? referenceId,
        DateTime createdDate, Content? content = null) : base(id)
    {
        Title = title;
        NotificationType = notificationType;
        ReferenceId = referenceId;
        CreatedDate = createdDate;
        Content = content;
    }

    public static Notification Create(Title title, NotificationType notificationType, ReferenceId? referenceId,
        DateTime createdDate, Content? content = null, bool sentNotification = false)
    {
        var noti = new Notification(NotificationId.New, title, notificationType, referenceId, createdDate, content);
        if (sentNotification)
        {
            noti.RaiseDomainEvent(new SendNotificationEvent(noti.Id));
        }

        return noti;
    }
}