namespace Domain.MemberNotifications;

public record MemberNotificationId(Guid Value)
{
    public static MemberNotificationId New => new MemberNotificationId(Guid.NewGuid());
}