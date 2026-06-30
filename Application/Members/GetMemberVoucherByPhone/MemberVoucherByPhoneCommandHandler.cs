using Application.Abstractions.Authentication;
using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.MemberVoucher.Response;
using Domain.Abstractions;
using Domain.Members;
using Domain.MemberVouchers;
using Microsoft.EntityFrameworkCore;

namespace Application.Members.GetMemberVoucherByPhone;

public class
    MemberVoucherByPhoneCommandHandler : ICommandHandler<MemberVoucherByPhoneCommand, List<MemberVoucherResponse>>
{
    private readonly IMemberVoucherRepository _memberVoucherRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IMemberContext _memberContext;
    private readonly IAwsS3Service _awsS3Service;

    public MemberVoucherByPhoneCommandHandler(IMemberVoucherRepository memberVoucherRepository,
        IMemberRepository memberRepository, IMemberContext memberContext, IAwsS3Service awsS3Service)
    {
        _memberVoucherRepository = memberVoucherRepository;
        _memberRepository = memberRepository;
        _memberContext = memberContext;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<List<MemberVoucherResponse>>> Handle(MemberVoucherByPhoneCommand request,
        CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByPhoneNumberAsync(request.PhoneNumber, cancellationToken);
        if (member is null)
            return Result.Failure<List<MemberVoucherResponse>>(new Error("Member.NotFound",
                "Không tìm thấy Customer"));
        var query = _memberVoucherRepository.GetEntitiesAsQueryable()
            .Include(x => x.Voucher)
            .Where(x => x.MemberId.Equals(member.Id));

        var memberVouchers = query.ToList().Select(x => new MemberVoucherResponse()
        {
            Status = x.MemberVoucherStatus,
            Id = x.Voucher.Id.Value,
            Point = x.Voucher.Point,
            StartedDate = x.Voucher.StartedDate,
            EndedDate = x.Voucher.EndedDate,
            TitleVoucher = x.Voucher.TitleVoucher.Value,
            ImageUrl = _awsS3Service.GetUrlPresign(x.Voucher.ImageUrl.Value, 60),
            ContentVoucher = x.Voucher.ContentVoucher != null ? x.Voucher.ContentVoucher.Value : string.Empty,
            CreatedDate = x.Voucher.CreatedDate.Date,
            QrCode = x.Voucher.QrCode != null ? x.Voucher.QrCode.Value : string.Empty,
            Conditions = x.Voucher.Conditions != null ? x.Voucher.Conditions.Value : string.Empty,
            QrCodeImageUrl = x.Voucher.QrCodeImageUrl != null
                ? _awsS3Service.GetUrlPresign(x.Voucher.QrCodeImageUrl.Value, 60)
                : string.Empty,
            Place = x.Voucher.Place != null ? x.Voucher.Place.Value : null,
            LimitQuantity = x.Voucher.LimitQuantity,
            PartnerName = x.Voucher.Partner != null ? x.Voucher.Partner.PartnerName.Value : String.Empty,
            DiscountValue = x.Voucher.DiscountValue,
            DiscountPercent = x.Voucher.DiscountPercent,
            MaxDiscountValue = x.Voucher.MaxDiscountValue,
            MinOrderValue = x.Voucher.MinOrderValue
        }).ToList();
        return Result.Success(memberVouchers);
    }
}