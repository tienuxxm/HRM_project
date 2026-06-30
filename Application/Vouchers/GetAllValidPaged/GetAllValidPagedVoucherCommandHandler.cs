using Application.Abstractions.Authentication;
using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Vouchers.GetAllPaged;
using Application.Vouchers.GetOne;
using Domain.Abstractions;
using Domain.Members;
using Domain.Vouchers;
using Domain.MemberVouchers;
using Microsoft.EntityFrameworkCore;

namespace Application.Vouchers.GetAllValid;

public class
    GetAllValidPagedVoucherCommandHandler : ICommandHandler<GetAllValidPagedVoucherCommand, GetAllVoucherPagedResposne>
{
    private readonly IVoucherRepository _voucherRepository;
    private readonly IMemberContext _memberContext;
    private readonly IMemberRepository _memberRepository;
    private readonly IMemberVoucherRepository _memberVoucherRepository;
    private readonly IAwsS3Service _awsS3Service;

    public GetAllValidPagedVoucherCommandHandler(IVoucherRepository voucherRepository, IMemberContext memberContext,
        IMemberRepository memberRepository, IMemberVoucherRepository memberVoucherRepository,
        IAwsS3Service awsS3Service)
    {
        _voucherRepository = voucherRepository;
        _memberContext = memberContext;
        _memberRepository = memberRepository;
        _memberVoucherRepository = memberVoucherRepository;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<GetAllVoucherPagedResposne>> Handle(GetAllValidPagedVoucherCommand request,
        CancellationToken cancellationToken)
    {
        Member? member = null;
        try
        {
            member = await _memberRepository.GetByIdentityAsync(_memberContext.IdentityId, cancellationToken);
        }
        catch
        {
            // ignored
        }

        var memberVouchers = new List<VoucherId>();

        if (member is not null)
        {
            memberVouchers = await _memberVoucherRepository.GetEntitiesAsQueryable()
                .Where(x => x.MemberId.Equals(member.Id))
                .Select(x => x.VoucherId)
                .ToListAsync(cancellationToken);
        }

        var query = _voucherRepository.GetEntitiesAsQueryable()
            .Include(x => x.Partner)
            .Where(v => memberVouchers.All(mv => !mv.Equals(v.Id)) &&
                        (!v.IsDelete.HasValue || !v.IsDelete.Value) &&
                        (!v.IsVoucherDefault.HasValue || !v.IsVoucherDefault.Value))
            .AsQueryable();

        query = !request.IncludePartner ? query.Where(x => x.PartnerId == null) : query.Where(x => x.PartnerId != null);
        query = query.OrderBy(x => x.Index)
            .ThenByDescending(x => x.CreatedDate);

        var data = await _voucherRepository.GetAllPaged(request, query);
        var voucherResponse = data.Data.Where(x => !x.IsExpired)
            .Select(x => new VoucherResponse()
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
                Place = x.Place?.Value,
                Status = x.Status,
                LimitQuantity = x.LimitQuantity,
                PartnerName = x.Partner != null ? x.Partner.PartnerName.Value : String.Empty,
                QrCodeId = x.QrCodeId,
                DiscountValue = x.DiscountValue,
                DiscountPercent = x.DiscountPercent,
                MaxDiscountValue = x.MaxDiscountValue,
                MinOrderValue = x.MinOrderValue,
                Index = x.Index
            }).ToList();

        return Result.Success(new GetAllVoucherPagedResposne(voucherResponse, data.TotalCount, data.CurrentPage,
            data.PageSize));
    }
}