using Application.Abstractions.FirebaseMessaging;
using Application.Abstractions.Messaging;
using Application.Extensions;
using Application.QrCode;
using Domain.Abstractions;
using Domain.MemberActivities;
using Domain.MemberDeviceTokens;
using Domain.MemberNotifications;
using Domain.Members;
using Domain.MemberVouchers;
using Domain.Notifications;
using Domain.QrCode;
using Domain.Shared;
using Domain.Vouchers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Content = Domain.MemberNotifications.Content;
using ReferenceId = Domain.MemberNotifications.ReferenceId;

namespace Application.Jobs.AppJob;

public class AppJobCommandHandler : ICommandHandler<AppJobCommand>
{
    private readonly IFirebaseMessaging _firebaseMessaging;
    private readonly IMemberActivityRepository _memberActivityRepository;
    private readonly IMemberDeviceTokenRepository _memberDeviceTokenRepository;
    private readonly IMemberNotificationRepository _memberNotificationRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IMemberVoucherRepository _memberVoucherRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IQrCodeRepository _qrCodeRepository;
    private readonly ISender _sender;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVoucherRepository _voucherRepository;

    public AppJobCommandHandler(IMemberRepository memberRepository,
        IMemberNotificationRepository memberNotificationRepository, INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork, IVoucherRepository voucherRepository, IMemberVoucherRepository memberVoucherRepository,
        IQrCodeRepository qrCodeRepository, ISender sender, IMemberActivityRepository memberActivityRepository,
        IMemberDeviceTokenRepository memberDeviceTokenRepository, IFirebaseMessaging firebaseMessaging)
    {
        _memberRepository = memberRepository;
        _memberNotificationRepository = memberNotificationRepository;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _voucherRepository = voucherRepository;
        _memberVoucherRepository = memberVoucherRepository;
        _qrCodeRepository = qrCodeRepository;
        _sender = sender;
        _memberActivityRepository = memberActivityRepository;
        _memberDeviceTokenRepository = memberDeviceTokenRepository;
        _firebaseMessaging = firebaseMessaging;
    }

    public async Task<Result> Handle(AppJobCommand request, CancellationToken cancellationToken)
    {
        using var transaction = _unitOfWork.BeginTransaction();
        var members = (await _memberRepository.GetEntitiesAsQueryable()
                .Include(x => x.MemberVouchers)
                .Include(x => x.MembershipClass)
                .Where(x => x.BirthDate.HasValue)
                .ToListAsync(cancellationToken))
            .Where(x => x.BirthDate.HasValue &&
                        (new DateTime(DateTime.Now.Year, x.BirthDate.Value.Month, x.BirthDate.Value.Day) -
                         DateTime.Today.AddDays(10).Date).Days <= 0 &&
                        (new DateTime(DateTime.Now.Year, x.BirthDate.Value.Month, x.BirthDate.Value.Day) -
                         DateTime.Today.AddDays(10).Date).Days >= -10 &&
                        (!x.SendBirthDateNotificationDate.HasValue ||
                         x.SendBirthDateNotificationDate.Value.Year != DateTime.Now.Year)).ToList();
        var memberNotifications = members.Select(m => MemberNotification.Create(m.Id,
            new Title($"Chúc mừng sinh nhật Customer {m.FullName}"),
            new Content($"Chúc mừng sinh nhật Customer {m.FullName}"),
            new MemberNotificationType(NotificationTypes.Member),
            new ReferenceId(m.Id.Value), DateTime.UtcNow)).ToList();

        var birthdateDefaultVoucher = await _voucherRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(x => x.IsVoucherDefault.HasValue && x.IsVoucherDefault.Value &&
                                      x.VoucherDefaultType == VoucherDefaultType.MemberBirthdate,
                cancellationToken);

        var memberFirebaseMessage = new Dictionary<string, string>();
        if (birthdateDefaultVoucher is not null)
            foreach (var member in members)
            {
                var memberBirthdateVoucher = Voucher.Clone(birthdateDefaultVoucher);
                memberBirthdateVoucher.SetDefaultVoucher(DateTime.UtcNow, DateTime.UtcNow.AddDays(30),
                    birthdateDefaultVoucher.ContentVoucher!.Value.Replace("{percent_number}",
                        member?.MembershipClass?.ClassName.Value == "Platinum" ? "15" : "10"),
                    birthdateDefaultVoucher.TitleVoucher.Value.Replace("{percent_number}",
                        member?.MembershipClass?.ClassName.Value == "Platinum" ? "15" : "10"),
                    member?.MembershipClass?.ClassName.Value == "Platinum" ? 15 : 10);
                memberBirthdateVoucher.SetUserVoucher();

                // var qrCodeId = Domain.QrCode.QrCode.Create(new QrCodeLinkId(memberBirthdateVoucher.Id.Value),
                //     QrCodeType.VOUCHER);
                // _qrCodeRepository.Add(qrCodeId);
                // memberBirthdateVoucher.SetQrCodeId(qrCodeId.Id.Value.ToString());
                var qrCode = Extention.GenerateRandomString(6, "WnzVC");
                var qrCodeImageCommand =
                    new GenerateQrCodeCommand("Voucher", memberBirthdateVoucher.Id.ToString(), qrCode);
                var qrCodeResult = await _sender.Send(qrCodeImageCommand, cancellationToken);
                if (qrCodeResult.IsSuccess)
                    memberBirthdateVoucher.SetQrCode(new ImageUrl(qrCodeResult.Value),
                        new Domain.Vouchers.QrCode(qrCode));

                var qrCodeId = Domain.QrCode.QrCode.Create(new QrCodeLinkId(memberBirthdateVoucher.Id.Value),
                    QrCodeType.VOUCHER);
                _qrCodeRepository.Add(qrCodeId);
                memberBirthdateVoucher.SetQrCodeId(qrCodeId.Id.Value.ToString());
                _voucherRepository.Add(memberBirthdateVoucher);
                var memberVoucher =
                    Domain.MemberVouchers.MemberVoucher.Create(member.Id, memberBirthdateVoucher.Id);
                _memberVoucherRepository.Add(memberVoucher);
                member.ClaimVoucher(memberVoucher);
                member.SetSendBirthDateNotificationDate(DateTime.UtcNow);
                var memberBirthdateVoucherNotifications = MemberNotification.Create(member.Id,
                    new Title(
                        $"Chúc mừng bạn {member.FullName} vừa được tặng một voucher nhân dịp ngày sinh nhật của mình"),
                    new Content(
                        $"Mừng sinh nhật Homies, Warning Zone tặng riêng Homies Voucher Giảm {(member?.MembershipClass?.ClassName.Value == "Platinum" ? "15" : "10")}% áp dụng cho hoá đơn đồ ăn"),
                    new MemberNotificationType(NotificationTypes.Voucher),
                    new ReferenceId(memberBirthdateVoucher.Id.Value), DateTime.UtcNow);
                memberNotifications.Add(memberBirthdateVoucherNotifications);
                if (member != null && !string.IsNullOrEmpty(member?.IdentityId))
                {
                    memberFirebaseMessage.Add(member.IdentityId, memberBirthdateVoucherNotifications.Content.Value);
                }
            }

        if (memberNotifications.Count > 0) _memberNotificationRepository.AddRange(memberNotifications);

        var notifications = members.Select(m =>
                Notification.Create(
                    new Title($"Hôm nay là sinh nhật Customer {m.FullName} - {m.PhoneNumber.Value}"),
                    new NotificationType(NotificationTypes.Member),
                    new Domain.Notifications.ReferenceId(m.Id.Value),
                    DateTime.UtcNow))
            .ToList();


        _notificationRepository.AddRange(notifications);
        _memberRepository.UpdateRange(members);
        //await HandleRemindExpireVoucher(cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        transaction.Commit();

        var deviceToken =
            await _memberDeviceTokenRepository.GetDeviceTokenAsync(
                memberFirebaseMessage.Select(x => x.Key).ToList(), cancellationToken);
        await _firebaseMessaging.SendMultipleNotification(deviceToken.Select(x => new FirebaseMessageRequest()
        {
            DeviceToken = x.DeviceToken,
            Message = memberFirebaseMessage[x.IdentityId]
        }).ToList());


        var identityIds = members.Where(x => !string.IsNullOrEmpty(x.IdentityId)).Select(x => x.IdentityId ?? "")
            .ToList();

        var deviceTokens =
            await _memberDeviceTokenRepository.GetDeviceTokenAsync(identityIds, cancellationToken);
        if (deviceTokens.Any())
        {
            var messages = deviceTokens.Select(token => new FirebaseMessageRequest
            {
                DeviceToken = token.DeviceToken,
                Message = "Chúc mừng sinh nhật bạn, Bạn được tặng 1 voucher ưu đãi"
            }).ToList();
            await _firebaseMessaging.SendMultipleNotification(messages);
        }


        // foreach (var token in deviceToken.Distinct())
        //     await _firebaseMessaging.SendNotification(token,
        //         "Chúc mừng sinh nhật bạn, Bạn được tặng 1 voucher ưu đãi");
        return Result.Success();
    }

    private async Task HandleRemindExpireVoucher(CancellationToken cancellationToken)
    {
        var allValidVoucher = await _voucherRepository.GetEntitiesAsQueryable()
            .Where(x => x.IsActive && (!x.IsDelete.HasValue || !x.IsDelete.Value) &&
                        x.EndedDate.Date >= DateTime.UtcNow.Date)
            .ToListAsync(cancellationToken);

        var allVoucherWillExpiredFor5DaysIds = allValidVoucher.Select(x => new
        {
            Offset = (x.EndedDate - DateTime.Now).Days,
            x.Id
        }).Where(x => x.Offset >= 0 && x.Offset <= 5).ToList();

        var allMemberVoucherWillExpiredFor5DaysIds = (await _memberVoucherRepository
                .GetEntitiesAsQueryable()
                .Where(x => !x.IsUsed && !x.IsVoucherExpired).ToListAsync(cancellationToken)).Where(
                x => allVoucherWillExpiredFor5DaysIds.Any(k => k.Id.Equals(x.VoucherId)))
            .ToList();
    }
}