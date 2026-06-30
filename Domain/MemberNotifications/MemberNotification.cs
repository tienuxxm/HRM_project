using Domain.Abstractions;
using Domain.MemberNotifications.Events;
using Domain.Members;
using Domain.Shared;

namespace Domain.MemberNotifications;

public class MemberNotification : Entity<MemberNotificationId>
{
    public MemberId MemberId { get; private set; }
    public Title Title { get; private set; }
    public Content Content { get; private set; }
    public MemberNotificationType NotificationType { get; private set; }
    public ReferenceId? ReferenceId { get; set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsReaded { get; private set; }

    private MemberNotification()
    {
    }

    private MemberNotification(MemberNotificationId id, MemberId memberId, Title title, Content content,
        MemberNotificationType notificationType, ReferenceId referenceId, DateTime createdAt) : base(id)
    {
        MemberId = memberId;
        Title = title;
        Content = content;
        NotificationType = notificationType;
        ReferenceId = referenceId;
        CreatedAt = createdAt;
        IsReaded = false;
    }

    public static MemberNotification Create(MemberId memberId, Title title, Content content,
        MemberNotificationType notificationType, ReferenceId referenceId, DateTime createdAt)
    {
        var notification = new MemberNotification(MemberNotificationId.New, memberId, title, content, notificationType,
            referenceId,
            createdAt);
        //notification.RaiseDomainEvent(new MemberNotificationCreateEvent(notification.Id));
        return notification;
    }

    public void Read()
    {
        IsReaded = true;
    }
}