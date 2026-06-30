using Domain.Abstractions;

namespace Domain.MemberNotifications.Events;

public record MemberNotificationCreateEvent(MemberNotificationId MemberNotificationId) : IDomainEvent;