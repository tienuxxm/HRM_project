using System.Diagnostics;
using Application.Abstractions.Clock;
using Application.Abstractions.FirebaseMessaging;
using Domain.Abstractions;
using Domain.Bookings;
using Domain.Bookings.Events;
using Domain.MemberDeviceTokens;
using Domain.MemberNotifications;
using Domain.Members;
using Domain.Notifications;
using Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Content = Domain.MemberNotifications.Content;

namespace Application.Bookings.ReserveBooking;

internal sealed class BookingReservedDomainEventHandler : INotificationHandler<BookingReservedDomainEvent>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly INotificationRepository _notificationRepository;
    private readonly IMemberNotificationRepository _memberNotificationRepository;
    private readonly IFirebaseMessaging _firebaseMessaging;
    private readonly IMemberRepository _memberRepository;
    private readonly IMemberDeviceTokenRepository _memberDeviceTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BookingReservedDomainEventHandler(
        IBookingRepository bookingRepository, IDateTimeProvider dateTimeProvider,
        INotificationRepository notificationRepository,
        IMemberNotificationRepository memberNotificationRepository, IUnitOfWork unitOfWork,
        IFirebaseMessaging firebaseMessaging, IMemberRepository memberRepository,
        IMemberDeviceTokenRepository memberDeviceTokenRepository)
    {
        _bookingRepository = bookingRepository;
        _dateTimeProvider = dateTimeProvider;
        _notificationRepository = notificationRepository;
        _memberNotificationRepository = memberNotificationRepository;
        _unitOfWork = unitOfWork;
        _firebaseMessaging = firebaseMessaging;
        _memberRepository = memberRepository;
        _memberDeviceTokenRepository = memberDeviceTokenRepository;
    }

    public async Task Handle(BookingReservedDomainEvent notification, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository
            .GetEntitiesAsQueryable()
            .Include(x => x.Restaurant)
            .Include(x => x.Member)
            .FirstOrDefaultAsync(x => x.Id.Equals(notification.BookingId), cancellationToken);

        if (booking is null)
            return;

        var bookingNotification = Notification.Create(
            new Title(
                $"Customer {booking.Member.FullName} vừa đặt bàn tại chi nhánh {booking.Restaurant.RestaurantName.Value}"),
            new NotificationType(NotificationTypes.Booking),
            new Domain.Notifications.ReferenceId(booking.Id.Value),
            _dateTimeProvider.UtcNow);
        _notificationRepository.Add(bookingNotification);
        var memberNotification = MemberNotification.Create(booking.MemberId,
            new Title($"Bạn vừa đặt bàn thành công tại chi nhánh {booking.Restaurant.RestaurantName.Value}"),
            new Content(
                $"Bạn vừa đặt bàn thành công tại chi nhánh {booking.Restaurant.RestaurantName.Value}"),
            new MemberNotificationType(NotificationTypes.Booking),
            new Domain.MemberNotifications.ReferenceId(booking.Id.Value),
            _dateTimeProvider.UtcNow);
        _memberNotificationRepository.Add(memberNotification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var identityId = await _memberRepository.GetIdentityById(booking.MemberId, cancellationToken);
        if (!string.IsNullOrEmpty(identityId))
        {
            var deviceToken = await _memberDeviceTokenRepository.GetByIdentityId(identityId, cancellationToken);
            if (string.IsNullOrEmpty(deviceToken?.DeviceToken))
                return;
            await _firebaseMessaging.SendNotification(deviceToken.DeviceToken,
                $"Bạn vừa đặt bàn thành công tại chi nhánh {booking.Restaurant.RestaurantName.Value}");
        }
    }
}