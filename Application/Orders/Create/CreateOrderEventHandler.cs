using Application.Abstractions.Clock;
using Domain.Abstractions;
using Domain.MemberNotifications;
using Domain.Notifications;
using Domain.Orders;
using Domain.Orders.Events;
using Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReferenceId = Domain.Notifications.ReferenceId;

namespace Application.Orders.Create;

internal sealed class CreateOrderEventHandler : INotificationHandler<CreateOrderEvent>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMemberNotificationRepository _memberNotificationRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderEventHandler(INotificationRepository notificationRepository, IOrderRepository orderRepository,
        IUnitOfWork unitOfWork, IMemberNotificationRepository memberNotificationRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _notificationRepository = notificationRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _memberNotificationRepository = memberNotificationRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Handle(CreateOrderEvent notification, CancellationToken cancellationToken)
    {
        var orderDetail = await _orderRepository.GetEntitiesAsQueryable()
            .Where(x => x.Id.Equals(notification.OrderId)).FirstOrDefaultAsync(cancellationToken);

        if (orderDetail is null)
            return;
        var orderNotification = Notification.Create(new Title($"Đơn hàng {orderDetail.OrderCode.Value} vừa đc tạo"),
            new NotificationType(NotificationTypes.Order),
            new ReferenceId(orderDetail.Id.Value),
            orderDetail.CreatedDate);
        _notificationRepository.Add(orderNotification);
        // if (orderDetail.OrderType == OrderType.Delivery)
        // {
        //     var memberNotification = MemberNotification.Create(orderDetail.MemberId,
        //         new Title(
        //             $"Quý khách đã đặt hàng thành công đơn hàng {orderDetail.OrderCode.Value}. Cảm ơn quý khách đã sử dụng dịch vụ tại Warning Zone"),
        //         new Content(
        //             $"Quý khách đã đặt hàng thành công đơn hàng {orderDetail.OrderCode.Value}. Cảm ơn quý khách đã sử dụng dịch vụ tại Warning Zone"),
        //         new MemberNotificationType(NotificationTypes.Order), new ReferenceId(orderDetail.Id.Value),
        //         _dateTimeProvider.UtcNow);
        //     _memberNotificationRepository.Add(memberNotification);
        // }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}