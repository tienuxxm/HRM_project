using Application.Abstractions.FirebaseMessaging;
using Domain.MemberDeviceTokens;
using Domain.MemberNotifications;
using Domain.MemberNotifications.Events;
using Domain.Members;
using MediatR;
using Newtonsoft.Json;

namespace Application.MemberNotifications;

public class MemberNotificationCreateEventHandler : INotificationHandler<MemberNotificationCreateEvent>
{
    private readonly IFirebaseMessaging _firebaseMessaging;
    private readonly IMemberDeviceTokenRepository _memberDeviceTokenRepository;
    private readonly IMemberNotificationRepository _memberNotificationRepository;
    private readonly IMemberRepository _memberRepository;

    public MemberNotificationCreateEventHandler(IMemberNotificationRepository memberNotificationRepository,
        IMemberDeviceTokenRepository memberDeviceTokenRepository, IMemberRepository memberRepository,
        IFirebaseMessaging firebaseMessaging)
    {
        _memberNotificationRepository = memberNotificationRepository;
        _memberDeviceTokenRepository = memberDeviceTokenRepository;
        _memberRepository = memberRepository;
        _firebaseMessaging = firebaseMessaging;
    }

    public async Task Handle(MemberNotificationCreateEvent notification, CancellationToken cancellationToken)
    {
        var memberNotification =
            await _memberNotificationRepository.GetByIdAsync(notification.MemberNotificationId, cancellationToken);
        if (memberNotification is null)
            return;

        var memberIdentity = await _memberRepository.GetIdentityById(memberNotification.MemberId, cancellationToken);
        if (string.IsNullOrEmpty(memberIdentity))
            return;
        var deviceTokens =
            await _memberDeviceTokenRepository.GetDeviceTokenAsync(new List<string> { memberIdentity },
                cancellationToken);
        if (deviceTokens is { Count: 0 })
            return;
        var message = JsonConvert.SerializeObject(memberNotification);
        var messageRequestList = deviceTokens.Select(token => new FirebaseMessageRequest
        {
            DeviceToken = token.DeviceToken,
            Message = message
        }).ToList();
        await _firebaseMessaging.SendMultipleNotification(messageRequestList);
    }
}