using Application.Abstractions.Messaging;

namespace Application.Notifications.CreateNotification;

public record CreateNotificationCommand(string Title, string Type, List<Guid> memberIds, string? Content) : ICommand;