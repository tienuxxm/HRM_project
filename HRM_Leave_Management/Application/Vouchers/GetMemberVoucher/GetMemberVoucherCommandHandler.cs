using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Vouchers.GetOne;
using Domain.Abstractions;
using Domain.Members;
using Domain.MemberVouchers;
using Domain.Vouchers;
using Microsoft.EntityFrameworkCore;

namespace Application.Vouchers.GetMemberVoucher;

public class GetMemberVoucherCommandHandler : ICommandHandler<GetMemberVoucherCommand, VoucherResponse>
{
    private readonly IAwsS3Service _awsS3Service;
    private readonly IMemberRepository _memberRepository;
    private readonly IMemberVoucherRepository _memberVoucherRepository;

    public GetMemberVoucherCommandHandler(IMemberVoucherRepository memberVoucherRepository,
        IMemberRepository memberRepository, IAwsS3Service awsS3Service)
    {
        _memberVoucherRepository = memberVoucherRepository;
        _memberRepository = memberRepository;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<VoucherResponse>> Handle(GetMemberVoucherCommand request,
        CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByPhoneNumberAsync(request.PhoneNumber.Value, cancellationToken);
        if (member is null)
            return Result.Failure<VoucherResponse>(MemberErrors.NotFound);
        var memberVoucher = await _memberVoucherRepository.GetEntitiesAsQueryable()
            .Include(x => x.Voucher)
            .Where(x => x.MemberId.Equals(member.Id) && x.VoucherId.Equals(request.VoucherId))
            .Select(x => x.Voucher)
            .Select(x => new VoucherResponse
            {
                Id = x.Id.Value,
                Point = x.Point,
                StartedDate = x.StartedDate,
                EndedDate = x.EndedDate,
                TitleVoucher = x.TitleVoucher.Value,
                ImageUrl = _awsS3Service.GetUrlPresign(x.ImageUrl.Value, 60),
                ContentVoucher = x.ContentVoucher != null ? x.ContentVoucher.Value : string.Empty,
                CreatedDate = x.CreatedDate.Date,
                QrCode = x.QrCode != null ? x.QrCode.Value : string.Empty,
                Conditions = x.Conditions != null ? x.Conditions.Value : string.Empty,
                QrCodeImageUrl = x.QrCodeImageUrl != null
                    ? _awsS3Service.GetUrlPresign(x.QrCodeImageUrl.Value, 60)
                    : string.Empty,
                Place = x.Place == null ? null : x.Place.Value,
                Status = x.Status,
                LimitQuantity = x.LimitQuantity,
                PartnerName = x.Partner != null ? x.Partner.PartnerName.Value : string.Empty,
                QrCodeId = x.QrCodeId,
                DiscountValue = x.DiscountValue,
                DiscountPercent = x.DiscountPercent,
                MaxDiscountValue = x.MaxDiscountValue,
                MinOrderValue = x.MinOrderValue,
                Index = x.Index
            }).FirstOrDefaultAsync(cancellationToken);
        if (memberVoucher is null)
            return Result.Failure<VoucherResponse>(VoucherErrors.NotFound);

        return Result.Success(memberVoucher);
    }
}