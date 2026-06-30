using Application.Abstractions.Clock;
using Application.Abstractions.FirebaseMessaging;
using Domain.Abstractions;
using Domain.MemberDeviceTokens;
using Domain.MemberNotifications;
using Domain.Members;
using Domain.Notifications;
using Domain.Orders;
using Domain.Orders.Events;
using Domain.Shared;
using MediatR;
using Content = Domain.MemberNotifications.Content;
using ReferenceId = Domain.MemberNotifications.ReferenceId;

namespace Application.Orders.Events;

public class ProcessOrderEventHandler : INotificationHandler<ProcessOrderEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMemberDeviceTokenRepository _memberDeviceTokenRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IFirebaseMessaging _messaging;
    private readonly IMemberNotificationRepository _memberNotificationRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ProcessOrderEventHandler(IOrderRepository orderRepository, IMemberRepository memberRepository,
        IMemberDeviceTokenRepository memberDeviceTokenRepository, IFirebaseMessaging messaging,
        IMemberNotificationRepository memberNotificationRepository, INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    {
        _orderRepository = orderRepository;
        _memberRepository = memberRepository;
        _memberDeviceTokenRepository = memberDeviceTokenRepository;
        _messaging = messaging;
        _memberNotificationRepository = memberNotificationRepository;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Handle(ProcessOrderEvent notification, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(notification.OrderId, cancellationToken);
        if (order is null)
            return;
        var member = await _memberRepository.GetByIdAsync(order.MemberId, cancellationToken);
        if (member is null)
            return;
        var identity = member.IdentityId;
        if (string.IsNullOrEmpty(identity))
            return;
        var message = $"Đơn hàng {order.OrderCode.Value} đang được thực hiện";
        var memberDeviceToken = await _memberDeviceTokenRepository.GetByIdentityId(identity, cancellationToken);
        if (memberDeviceToken is not null)
        {
            await _messaging.SendNotification(memberDeviceToken.DeviceToken, message);
        }

        var memberNotification = MemberNotification.Create(member.Id, new Title(message), new Content(message),
            new MemberNotificationType(NotificationTypes.Order), new ReferenceId(order.Id.Value),
            _dateTimeProvider.UtcNow);
        var systemNotification = Notification.Create(new Title(message), new NotificationType(NotificationTypes.Order),
            new Domain.Notifications.ReferenceId(order.Id.Value), _dateTimeProvider.UtcNow);
        _memberNotificationRepository.Add(memberNotification);
        _notificationRepository.Add(systemNotification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}