using Application.Abstractions.FirebaseMessaging;
using Domain.MemberDeviceTokens;
using Domain.Notifications;
using Domain.Notifications.Events;
using MediatR;
using Newtonsoft.Json;

namespace Application.Notifications.CreateNotification;

public class SendNotificationEventHandler : INotificationHandler<SendNotificationEvent>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IFirebaseMessaging _firebaseMessaging;
    private readonly IMemberDeviceTokenRepository _memberDeviceTokenRepository;

    public SendNotificationEventHandler(INotificationRepository notificationRepository,
        IFirebaseMessaging firebaseMessaging, IMemberDeviceTokenRepository memberDeviceTokenRepository)
    {
        _notificationRepository = notificationRepository;
        _firebaseMessaging = firebaseMessaging;
        _memberDeviceTokenRepository = memberDeviceTokenRepository;
    }

    public async Task Handle(SendNotificationEvent request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(request.NotificationId, cancellationToken);
        if (notification is null)
            return;
        var memberDeviceTokens = await _memberDeviceTokenRepository.GetAll(cancellationToken);
        if (memberDeviceTokens is null)
            return;
        var messages = memberDeviceTokens.Select(x => new FirebaseMessageRequest()
        {
            Message = JsonConvert.SerializeObject(notification),
            DeviceToken = x.DeviceToken
        }).ToList();
        await _firebaseMessaging.SendMultipleNotification(messages);
    }
}