using Application.Abstractions.Authentication;
using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.MemberNotifications;
using Domain.MemberPointHistories;
using Domain.Members;
using Domain.MemberVouchers;
using Domain.Notifications;
using Domain.Shared;
using Domain.Vouchers;
using Microsoft.EntityFrameworkCore;
using Content = Domain.MemberNotifications.Content;
using ReferenceId = Domain.MemberNotifications.ReferenceId;

namespace Application.Vouchers.ClaimVoucher;

public class ClaimVoucherCommandHandler : ICommandHandler<ClaimVoucherCommand, bool>
{
    private readonly IMemberContext _memberContext;
    private readonly IVoucherRepository _voucherRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IMemberPointHistoryRepository _memberPointHistoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMemberNotificationRepository _memberNotificationRepository;

    public ClaimVoucherCommandHandler(IMemberContext memberContext, IVoucherRepository voucherRepository,
        IMemberRepository memberRepository, IMemberPointHistoryRepository memberPointHistoryRepository,
        IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider,
        IMemberNotificationRepository memberNotificationRepository)
    {
        _memberContext = memberContext;
        _voucherRepository = voucherRepository;
        _memberRepository = memberRepository;
        _memberPointHistoryRepository = memberPointHistoryRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _memberNotificationRepository = memberNotificationRepository;
    }

    public async Task<Result<bool>> Handle(ClaimVoucherCommand request, CancellationToken cancellationToken)
    {
        using var transaction = _unitOfWork.BeginTransaction();
        try
        {
            var member =
                await _memberRepository.GetEntitiesAsQueryable()
                    .Include(x => x.MemberVouchers)
                    .Include(x => x.MemberPointHistories)
                    .FirstOrDefaultAsync(x => x.IdentityId == _memberContext.IdentityId, cancellationToken);
            if (member is null)
                return Result.Failure<bool>(MemberErrors.NotFound);
            var voucher = await _voucherRepository.GetByIdAsync(request.VoucherId, cancellationToken);
            if (voucher is null)
                return Result.Failure<bool>(VoucherErrors.NotFound);
            var voucherExisted = member.MemberVouchers.Any(x => x.VoucherId.Equals(voucher.Id));
            if (voucherExisted)
                return Result.Failure<bool>(MemberVoucherErrors.VoucherExisted);
            if (voucher.IsExpired)
                return Result.Failure<bool>(VoucherErrors.VoucherExpired);
            if (voucher.LimitQuantity is <= 0)
                return Result.Failure<bool>(VoucherErrors.VoucherOutOfRange);
            if (voucher.IsDelete.HasValue && voucher.IsDelete.Value)
                return Result.Failure<bool>(VoucherErrors.InvalidVoucher);

            var memberPoint = 0;
            if (voucher.Point > 0)
            {
                var memberHasPoint = await _memberPointHistoryRepository.GetEntitiesAsQueryable()
                    .AnyAsync(x => x.MemberId.Equals(member.Id), cancellationToken);
                if (!memberHasPoint)
                    return Result.Failure<bool>(VoucherErrors.NotEnoughPoint);
                memberPoint = _memberPointHistoryRepository.GetEntitiesAsQueryable()
                    .Where(x => x.MemberId.Equals(member.Id))
                    .Select(x => x.MemberPoint)
                    .AsEnumerable()
                    .Aggregate((x, y) => x + y).Value;
                if (memberPoint < voucher.Point)
                    return Result.Failure<bool>(VoucherErrors.NotEnoughPoint);
            }

            var memberVoucher = Domain.MemberVouchers.MemberVoucher.Create(member.Id, voucher.Id);
            member.ClaimVoucher(memberVoucher);
            var memberPointHistory = MemberPointHistory.Create(member.Id, new MemberPoint(-voucher.Point),
                PointType.USED, new Title($"Đổi thành công voucher {voucher.TitleVoucher.Value}"),
                _dateTimeProvider.UtcNow);
            member.AddMemberPoint(memberPointHistory);
            voucher.DescreaseQuantity();
            _voucherRepository.Update(voucher);
            var notification = MemberNotification.Create(member.Id,
                new Title($"Đổi thành công voucher {voucher.TitleVoucher.Value}"),
                new Content($"Chúc mừng bạn đã đổi thành công voucher {voucher.TitleVoucher.Value}"),
                new MemberNotificationType(NotificationTypes.Voucher), new ReferenceId(voucher.Id.Value),
                DateTime.UtcNow);
            _memberNotificationRepository.Add(notification);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            transaction.Commit();
            return Result.Success(true);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            return Result.Failure<bool>(new Error("ClaimVoucher.Exception", "Có lỗi trong khi đổi voucher"));
        }
    }
}