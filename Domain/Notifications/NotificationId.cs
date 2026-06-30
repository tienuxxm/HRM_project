namespace Domain.Notifications;

public record NotificationId(Guid Value)
{
    public static NotificationId New => new NotificationId(Guid.NewGuid());
}