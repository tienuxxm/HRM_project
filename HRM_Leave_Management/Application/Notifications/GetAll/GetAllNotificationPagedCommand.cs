using Application.Abstractions.Messaging;
using Application.Notifications.Response;
using Domain.Abstractions;
using Domain.Notifications;

namespace Application.Notifications.GetAll;

public record GetAllNotificationPagedCommand() : PagedQuery<Notification, NotificationId>,
    ICommand<PagedList<NotificationResponse>>;