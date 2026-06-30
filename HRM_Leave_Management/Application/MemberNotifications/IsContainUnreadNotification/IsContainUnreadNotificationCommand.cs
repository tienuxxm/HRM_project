using Application.Abstractions.Messaging;

namespace Application.MemberNotifications.IsContainUnreadNotification;

public record IsContainUnreadNotificationCommand : ICommand<bool>;