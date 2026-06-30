using Application.Abstractions.Messaging;
using Domain.MemberNotifications;

namespace Application.MemberNotifications;

public record DeleteMemberNotificationCommand(MemberNotificationId Id):ICommand;