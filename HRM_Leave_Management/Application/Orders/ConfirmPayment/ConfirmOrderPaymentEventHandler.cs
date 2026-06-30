using Application.Abstractions.Clock;
using Application.Extensions;
using Domain.Abstractions;
using Domain.MemberNotifications;
using Domain.Notifications;
using Domain.Orders;
using Domain.Orders.Events;
using Domain.Shared;
using MediatR;
using Content = Domain.MemberNotifications.Content;
using ReferenceId = Domain.MemberNotifications.ReferenceId;

namespace Application.Orders.ConfirmPayment;

public class ConfirmOrderPaymentEventHandler : INotificationHandler<ConfirmOrderPaymentEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMemberNotificationRepository _memberNotificationRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmOrderPaymentEventHandler(IOrderRepository orderRepository,
        IMemberNotificationRepository memberNotificationRepository, IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork, INotificationRepository notificationRepository)
    {
        _orderRepository = orderRepository;
        _memberNotificationRepository = memberNotificationRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
        _notificationRepository = notificationRepository;
    }

    public async Task Handle(ConfirmOrderPaymentEvent notification, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(notification.OrderId, cancellationToken);
        if (order is null)
            return;
        var orderNotification = Notification.Create(
            new Title($"Đơn hàng {order.OrderCode.Value} vừa được thanh toán"),
            new NotificationType(NotificationTypes.Order),
            new Domain.Notifications.ReferenceId(order.Id.Value),
            _dateTimeProvider.UtcNow);
        _notificationRepository.Add(orderNotification);
        var memberNotification = MemberNotification.Create(order.MemberId,
            new Title($"Thanh toán thành công đơn hàng {order.OrderCode.Value}"),
            new Content(
                $"Đơn hành {order.OrderCode.Value} vừa được thanh toán thành công với số tiền {order.TotalBill.ToVndFormat()}"),
            new MemberNotificationType(NotificationTypes.Order), new ReferenceId(order.Id.Value),
            _dateTimeProvider.UtcNow);
        _memberNotificationRepository.Add(memberNotification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}