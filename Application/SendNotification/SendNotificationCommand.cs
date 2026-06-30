using Application.Abstractions.Messaging;

namespace Application.SendNotification;

public record SendNotificationCommand(string Token, string Message) : ICommand;