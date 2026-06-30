using Domain.Abstractions;

namespace Domain.Notifications.Events;

public record SendNotificationEvent(NotificationId NotificationId) : IDomainEvent;