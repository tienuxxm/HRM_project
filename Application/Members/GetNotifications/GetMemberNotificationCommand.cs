using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.MemberNotifications;

namespace Application.Members.GetNotifications;

public record GetMemberNotificationCommand() : PagedQuery<MemberNotification, MemberNotificationId>,
    ICommand<PagedList<MemberNotificationResponse>>;