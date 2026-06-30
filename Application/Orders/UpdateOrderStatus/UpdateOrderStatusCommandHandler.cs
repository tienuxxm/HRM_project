using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.MemberNotifications;
using Domain.MemberPointHistories;
using Domain.Members;
using Domain.MembershipClasses;
using Domain.Notifications;
using Domain.Orders;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Content = Domain.MemberNotifications.Content;
using ReferenceId = Domain.MemberNotifications.ReferenceId;

namespace Application.Orders.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler : ICommandHandler<UpdateOrderStatusCommand>
{
    private const int MembershipDefaultLevel = 0;
    private const int MembershipSilverLevel = 1;
    private const int MembershipGoldLevel = 2;
    private const int MembershipDiamondLevel = 3;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMemberNotificationRepository _memberNotificationRepository;
    private readonly IMemberPointHistoryRepository _memberPointHistoryRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IMembershipClassRepository _membershipClassRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrderStatusCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IMemberPointHistoryRepository memberPointHistoryRepository,
        IMembershipClassRepository membershipClassRepository, IMemberRepository memberRepository,
        IDateTimeProvider dateTimeProvider, IMemberNotificationRepository memberNotificationRepository)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _memberRepository = memberRepository;
        _dateTimeProvider = dateTimeProvider;
        _memberNotificationRepository = memberNotificationRepository;
        _memberPointHistoryRepository = memberPointHistoryRepository;
        _membershipClassRepository = membershipClassRepository;
        _memberRepository = memberRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var orderId = new OrderId(request.OrderId);
        var order = await _orderRepository.GetEntitiesAsQueryable().Include(o => o.Invoice)
            .FirstOrDefaultAsync(o => o.Id.Equals(orderId), cancellationToken);

        if (order is null)
            return Result.Failure(OrderErrors.NotFound);

        var result = request.OrderStatus switch
        {
            OrderStatus.Process => order.ProcessOrder(),
            OrderStatus.Shipping => order.ShippingOrder(),
            OrderStatus.Done => order.MarkDone(),
            OrderStatus.Cancel => order.CancelOrder(),
            _ => Result.Failure(new Error("UpdateOrder.Fail", "Fail to update order"))
        };

        if (result.IsFailure)
            return result;

        if (request.OrderStatus == OrderStatus.Done && order.Invoice is not null)
        {
            var member = await _memberRepository.GetEntitiesAsQueryable()
                .Include(m => m.MembershipClass)
                .Include(m => m.MemberPointHistories)
                .FirstOrDefaultAsync(x => x.Id.Equals(order.MemberId), cancellationToken);
            if (member is null)
                return Result.Failure(OrderErrors.NotFound);

            if (member.MembershipClass is not null)
            {
                var totalPoint = (int)Math.Floor(order.Invoice.TotalBill.Amount / 100000M);

                if (totalPoint > 0)
                {
                    var memberPointHistory = MemberPointHistory.Create(
                        member.Id,
                        new MemberPoint(totalPoint),
                        PointType.ADDED,
                        new Title("Cộng tích điểm đơn hàng " + order.OrderCode.Value),
                        _dateTimeProvider.UtcNow
                    );
                    _memberPointHistoryRepository.Add(memberPointHistory);
                }

                if (!member.MembershipAssignedDate.HasValue) member.AssignMembershipClass(member.MembershipClass);

                var ordersTotalBill = await _orderRepository.GetEntitiesAsQueryable()
                    .AsNoTracking()
                    .Where(x => member.MembershipAssignedDate != null && x.MemberId.Equals(member.Id) && x.HasPayment &&
                                x.PaymentDate.HasValue &&
                                x.PaymentDate.Value >= member.MembershipAssignedDate.Value)
                    .Select(x => x.TotalBill)
                    .ToListAsync(cancellationToken);

                var memberPoint = member?.MemberPointHistories?.Sum(x => x.MemberPoint.Value);
                var silverClass = await _membershipClassRepository.GetEntitiesAsQueryable()
                    .FirstOrDefaultAsync(
                        x => x.Level.Equals(new Level(MembershipSilverLevel)),
                        cancellationToken);
                var goldClass = await _membershipClassRepository.GetEntitiesAsQueryable()
                    .FirstOrDefaultAsync(
                        x => x.Level.Equals(new Level(MembershipGoldLevel)),
                        cancellationToken);
                var diamond = await _membershipClassRepository.GetEntitiesAsQueryable()
                    .FirstOrDefaultAsync(
                        x => x.Level.Equals(new Level(MembershipDiamondLevel)),
                        cancellationToken);
                MembershipClass? membershipClassWithPoint = null;
                if (memberPoint.HasValue)
                    switch (memberPoint.Value)
                    {
                        case >= 200 and < 500:
                            membershipClassWithPoint = silverClass;
                            break;
                        case >= 500 and < 1000:
                            membershipClassWithPoint = goldClass;
                            break;
                        case >= 1000:
                            membershipClassWithPoint = diamond;
                            break;
                    }

                MembershipClass? membershipClassWithMoney = null;
                var totalPaid = ordersTotalBill.Count > 0
                    ? ordersTotalBill.Aggregate((x, y) => x + y)
                    : Money.Zero(Currency.Vnd);
                switch (totalPaid.Amount)
                {
                    case >= 20000000 and < 50000000:
                        membershipClassWithMoney = silverClass;
                        break;
                    case >= 50000000 and < 100000000:
                        membershipClassWithMoney = goldClass;
                        break;
                    case >= 100000000:
                        membershipClassWithMoney = diamond;
                        break;
                }

                MembershipClass? newMembershipClass = null;
                if (membershipClassWithPoint != null && membershipClassWithMoney != null)
                    newMembershipClass = membershipClassWithPoint.Level.Value > membershipClassWithMoney.Level.Value
                        ? membershipClassWithPoint
                        : membershipClassWithMoney;
                else if (membershipClassWithPoint != null)
                    newMembershipClass = membershipClassWithPoint;
                else if (membershipClassWithMoney != null) newMembershipClass = membershipClassWithMoney;


                if (newMembershipClass != null)
                {
                    member?.AssignMembershipClass(newMembershipClass);
                    var memberNotification = MemberNotification.Create(member.Id,
                        new Title("Congratulations on Your Rank Up " +
                                  member.MembershipClass.ClassName.Value),
                        new Content("Congratulations on Your Rank Up " +
                                    member.MembershipClass.ClassName.Value),
                        new MemberNotificationType(NotificationTypes.Member), new ReferenceId(member.Id.Value),
                        DateTime.UtcNow);

                    _memberRepository.Update(member);
                    _memberNotificationRepository.Add(memberNotification);
                }
            }
        }

        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}