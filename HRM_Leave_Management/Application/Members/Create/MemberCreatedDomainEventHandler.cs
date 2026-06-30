using Application.Extensions;
using Application.QrCode;
using Domain.Abstractions;
using Domain.MemberNotifications;
using Domain.Members;
using Domain.Members.Events;
using Domain.MemberVouchers;
using Domain.Notifications;
using Domain.QrCode;
using Domain.Shared;
using Domain.Vouchers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Content = Domain.MemberNotifications.Content;
using ReferenceId = Domain.MemberNotifications.ReferenceId;

namespace Application.Members.Create;

public class MemberCreatedDomainEventHandler : INotificationHandler<MemberCreatedDomainEvent>
{
    private readonly IVoucherRepository _voucherRepository;
    private readonly IQrCodeRepository _qrCodeRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IMemberNotificationRepository _memberNotificationRepository;
    private readonly IMemberVoucherRepository _memberVoucherRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISender _sender;

    public MemberCreatedDomainEventHandler(IVoucherRepository voucherRepository, IQrCodeRepository qrCodeRepository,
        IMemberRepository memberRepository, IMemberNotificationRepository memberNotificationRepository,
        IMemberVoucherRepository memberVoucherRepository, IUnitOfWork unitOfWork, ISender sender)
    {
        _voucherRepository = voucherRepository;
        _qrCodeRepository = qrCodeRepository;
        _memberRepository = memberRepository;
        _memberNotificationRepository = memberNotificationRepository;
        _memberVoucherRepository = memberVoucherRepository;
        _unitOfWork = unitOfWork;
        _sender = sender;
    }

    public async Task Handle(MemberCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var member = await _memberRepository.GetEntitiesAsQueryable()
                .Include(x => x.MemberVouchers)
                .FirstOrDefaultAsync(x => x.Id == notification.MemberId, cancellationToken);
            if (member is null)
                return;
            var newMemberDefaultVoucher = await _voucherRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(x => x.IsVoucherDefault.HasValue && x.IsVoucherDefault.Value &&
                                          x.VoucherDefaultType == VoucherDefaultType.NewMemberRegisterd,
                    cancellationToken);
            if (newMemberDefaultVoucher is null)
            {
                await Task.CompletedTask;
                return;
            }

            var newMemberVoucher = Voucher.Clone(newMemberDefaultVoucher);
            newMemberVoucher.SetUserVoucher();
            newMemberVoucher.SetDefaultVoucher(DateTime.UtcNow, DateTime.UtcNow.AddDays(30),
                $"Warning Zone tặng Homies voucher giảm 100.000 VND áp dụng cho hoá đơn từ 500.000 VND khi lần đầu đăng nhập app",
                newMemberDefaultVoucher.TitleVoucher.Value, 0);

            var qrCode = Extention.GenerateRandomString(6, "WnzVC");
            var qrCodeImageCommand = new GenerateQrCodeCommand("Voucher", newMemberVoucher.Id.ToString(), qrCode);
            var qrCodeResult = await _sender.Send(qrCodeImageCommand, cancellationToken);
            if (qrCodeResult.IsSuccess)
                newMemberVoucher.SetQrCode(new ImageUrl(qrCodeResult.Value), new Domain.Vouchers.QrCode(qrCode));

            var qrCodeId = Domain.QrCode.QrCode.Create(new QrCodeLinkId(newMemberVoucher.Id.Value), QrCodeType.VOUCHER);
            _qrCodeRepository.Add(qrCodeId);
            newMemberVoucher.SetQrCodeId(qrCodeId.Id.Value.ToString());
            _voucherRepository.Add(newMemberVoucher);
            var memberVoucher =
                Domain.MemberVouchers.MemberVoucher.Create(member.Id, newMemberVoucher.Id);
            _memberVoucherRepository.Add(memberVoucher);
            member.ClaimVoucher(memberVoucher);
            member.SetSendBirthDateNotificationDate(DateTime.UtcNow);
            var memberBirthdateVoucherNotifications = MemberNotification.Create(member.Id,
                new Title(
                    $"Chúc mừng bạn {member.FullName} vừa được tặng một voucher khi đăng kí thành viên tại Warning Zone"),
                new Content(
                    $"Chúc mừng bạn {member.FullName} vừa được tặng một voucher khi đăng kí thành viên tại Warning Zone"),
                new MemberNotificationType(NotificationTypes.Voucher),
                new ReferenceId(newMemberVoucher.Id.Value), DateTime.UtcNow);
            _memberRepository.Update(member);
            _memberNotificationRepository.Add(memberBirthdateVoucherNotifications);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}