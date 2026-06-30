using Application.Abstractions.Messaging;
using Application.Notifications.Response;
using Domain.Abstractions;
using Domain.Notifications;

namespace Application.Notifications.GetAll;

public class
    GetAllNotificationPagedCommandHandler : ICommandHandler<GetAllNotificationPagedCommand,
        PagedList<NotificationResponse>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetAllNotificationPagedCommandHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<PagedList<NotificationResponse>>> Handle(GetAllNotificationPagedCommand request,
        CancellationToken cancellationToken)
    {
        var data = await _notificationRepository.GetAllPaged(request);
        var dataResponse = data.Data.Select(noti => new NotificationResponse()
        {
            Id = noti.Id.Value,
            Title = noti.Title.Value,
            Type = noti.NotificationType.Value,
            CreatedAt = noti.CreatedDate.Date,
            ReferenceId = noti.ReferenceId?.Value,
            Content = noti.Content?.Value
        }).ToList();

        return Result.Success(
            new PagedList<NotificationResponse>(dataResponse, data.TotalCount, data.CurrentPage, data.PageSize));
    }
}