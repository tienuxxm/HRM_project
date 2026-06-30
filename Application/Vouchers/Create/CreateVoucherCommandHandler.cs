using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Application.Extensions;
using Application.QrCode;
using Domain.Abstractions;
using Domain.Members;
using Domain.MembershipClasses;
using Domain.MemberVouchers;
using Domain.Partners;
using Domain.QrCode;
using Domain.Shared;
using Domain.Vouchers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Vouchers.Create;

internal class CreateVoucherCommandHandler : ICommandHandler<CreateVoucherCommand, Guid>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMemberRepository _memberRepository;
    private readonly IMemberVoucherRepository _memberVoucherRepository;
    private readonly IPartnerRepository _partnerRepository;
    private readonly IQrCodeRepository _qrCodeRepository;
    private readonly ISender _sender;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVoucherRepository _voucherRepository;
    private readonly IMembershipClassRepository _membershipClassRepository;

    public CreateVoucherCommandHandler(
        IVoucherRepository voucherRepository,
        IPartnerRepository partnerRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork, ISender sender, IQrCodeRepository qrCodeRepository, IMemberRepository memberRepository,
        IMemberVoucherRepository memberVoucherRepository, IMembershipClassRepository membershipClassRepository)
    {
        _voucherRepository = voucherRepository;
        _partnerRepository = partnerRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
        _sender = sender;
        _qrCodeRepository = qrCodeRepository;
        _memberRepository = memberRepository;
        _memberVoucherRepository = memberVoucherRepository;
        _membershipClassRepository = membershipClassRepository;
    }

    public async Task<Result<Guid>> Handle(CreateVoucherCommand request, CancellationToken cancellationToken)
    {
        using var trx = _unitOfWork.BeginTransaction();
        try
        {
            if (request.PartnerId.HasValue)
            {
                var partner =
                    await _partnerRepository.GetByIdAsync(new PartnerId(request.PartnerId.Value), cancellationToken);
                if (partner is null) return Result.Failure<Guid>(VoucherErrors.NotFound);
            }


            var voucher = Voucher.Create(
                new TitleVoucher(request.TitleVoucher),
                new ImageUrl(request.ImageUrl),
                request.StartedDate,
                request.EndedDate,
                request.Place != null ? new Place(request.Place) : null,
                request.Point,
                _dateTimeProvider.UtcNow,
                request.PartnerId.HasValue ? new PartnerId(request.PartnerId.Value) : null,
                request.ContentVoucher != null ? new ContentVoucher(request.ContentVoucher) : null,
                request.Conditions != null ? new Conditions(request.Conditions) : null,
                request.LimitQuantity,
                request.DiscountValue,
                request.DiscountPercent,
                request.MaxDiscountValue,
                request.MinOrderValue,
                request.Index,
                request.IsDefault,
                //Default true if has member code
                request.VoucherDefaultType,
                "",
                ""
            );

            var qrCode = Extention.GenerateRandomString(6, "WnzVC");
            var qrCodeImageCommand = new GenerateQrCodeCommand("Voucher", voucher.Id.ToString(), qrCode);
            var qrCodeResult = await _sender.Send(qrCodeImageCommand, cancellationToken);
            if (qrCodeResult.IsSuccess)
                voucher.SetQrCode(new ImageUrl(qrCodeResult.Value), new Domain.Vouchers.QrCode(qrCode));

            var qrCodeId = Domain.QrCode.QrCode.Create(new QrCodeLinkId(voucher.Id.Value), QrCodeType.VOUCHER);
            _qrCodeRepository.Add(qrCodeId);
            voucher.SetQrCodeId(qrCodeId.Id.Value.ToString());

            if (request?.MemberIds?.Length > 0)
            {
                voucher.SetUserVoucher();
                var memberIds = request.MemberIds.Select(x => new MemberId(new Guid(x))).ToList();
                var members = await _memberRepository.GetEntitiesAsQueryable().Where(x => memberIds.Contains(x.Id))
                    .ToListAsync(cancellationToken);
                var memberNames = string.Join(",", members.Select(x => x.FullName).ToArray());
                voucher.SetMembers(memberNames);
                foreach (var memberVoucher in members.Select(member =>
                             Domain.MemberVouchers.MemberVoucher.Create(member.Id, voucher.Id)))
                {
                    _memberVoucherRepository.Add(memberVoucher);
                }
            }
            else if (request?.MembershipIds?.Length > 0)
            {
                voucher.SetUserVoucher();
                var memberClassIds = request.MembershipIds.Select(x => new MembershipClassId(new Guid(x))).ToList();
                var membershipClasses = await _membershipClassRepository.GetEntitiesAsQueryable()
                    .Where(x => memberClassIds.Contains(x.Id)).ToListAsync(cancellationToken);
                var membershipClassNames = string.Join(",", membershipClasses.Select(x => x.ClassName.Value).ToArray());
                var members = await _memberRepository.GetEntitiesAsQueryable()
                    .Include(x => x.MembershipClass)
                    .Where(x => x.MembershipClass != null && memberClassIds.Contains(x.MembershipClass.Id))
                    .ToListAsync(cancellationToken);
                voucher.SetMemberClasses(membershipClassNames);
                foreach (var memberVoucher in members.Select(member =>
                             Domain.MemberVouchers.MemberVoucher.Create(member.Id, voucher.Id)))
                {
                    _memberVoucherRepository.Add(memberVoucher);
                }
            }

            _voucherRepository.Add(voucher);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            trx.Commit();
            return voucher.Id.Value;
        }
        catch (Exception _)
        {
            trx.Rollback();
            return Result.Failure<Guid>(VoucherErrors.InvalidVoucher);
        }
    }
}