using Application.Abstractions.Messaging;

namespace Application.Members.ReadMemberNotification;

public record ReadMemberNotificationCommand(Guid NotificationId) : ICommand;