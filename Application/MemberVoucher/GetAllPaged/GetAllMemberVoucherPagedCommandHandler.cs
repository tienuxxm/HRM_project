using Application.Abstractions.Authentication;
using Application.Abstractions.AWS;
using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Application.MemberVoucher.Response;
using Domain.Abstractions;
using Domain.Members;
using Domain.MemberVouchers;
using Microsoft.EntityFrameworkCore;

namespace Application.MemberVoucher.GetAllPaged;

public class
    GetAllMemberVoucherPagedCommandHandler : ICommandHandler<GetAllMemberVoucherPagedCommand,
    PagedList<MemberVoucherResponse>>
{
    private readonly IAwsS3Service _awsS3Service;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMemberContext _memberContext;
    private readonly IMemberRepository _memberRepository;
    private readonly IMemberVoucherRepository _memberVoucherRepository;

    public GetAllMemberVoucherPagedCommandHandler(IMemberVoucherRepository memberVoucherRepository,
        IMemberContext memberContext, IMemberRepository memberRepository, IDateTimeProvider dateTimeProvider,
        IAwsS3Service awsS3Service)
    {
        _memberVoucherRepository = memberVoucherRepository;
        _memberContext = memberContext;
        _memberRepository = memberRepository;
        _dateTimeProvider = dateTimeProvider;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<PagedList<MemberVoucherResponse>>> Handle(GetAllMemberVoucherPagedCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            Member? member;
            if (request.MemberId is null)
                member = await _memberRepository.GetByIdentityAsync(_memberContext.IdentityId, cancellationToken);
            else
                member = await _memberRepository.GetByIdAsync(request.MemberId, cancellationToken);
            if (member is null)
                return Result.Failure<PagedList<MemberVoucherResponse>>(MemberErrors.NotFound);
            var query = _memberVoucherRepository.GetEntitiesAsQueryable()
                .Include(x => x.Voucher).ThenInclude(x => x.Partner)
                .Where(x => x.MemberId.Equals(member.Id)).OrderByDescending(x => x.Voucher.CreatedDate).AsQueryable();
            if (request.MemberVoucherStatus.HasValue)
                query = request.MemberVoucherStatus switch
                {
                    MemberVoucherStatus.Available => query.Where(x =>
                        !x.IsUsed && x.Voucher.EndedDate.Date > _dateTimeProvider.UtcNow),
                    MemberVoucherStatus.Expired =>
                        query.Where(x => x.Voucher.EndedDate.Date < _dateTimeProvider.UtcNow),
                    MemberVoucherStatus.Used => query.Where(x => x.IsUsed),
                    _ => query
                };

            var result = await _memberVoucherRepository.GetAllPaged(request, query);
            var memberVouchers = result.Data.Select(x => new MemberVoucherResponse
            {
                Status = x.MemberVoucherStatus,
                Id = x.Voucher.Id.Value,
                Point = x.Voucher.Point,
                StartedDate = x.Voucher.StartedDate,
                EndedDate = x.Voucher.EndedDate,
                TitleVoucher = x.Voucher.TitleVoucher.Value,
                ImageUrl = _awsS3Service.GetUrlPresign(x.Voucher.ImageUrl.Value),
                ContentVoucher = x.Voucher.ContentVoucher != null ? x.Voucher.ContentVoucher.Value : string.Empty,
                CreatedDate = x.Voucher.CreatedDate.Date,
                QrCode = x.Voucher.QrCode != null ? x.Voucher.QrCode.Value : string.Empty,
                Conditions = x.Voucher.Conditions != null ? x.Voucher.Conditions.Value : string.Empty,
                QrCodeImageUrl = x.Voucher.QrCodeImageUrl != null
                    ? _awsS3Service.GetUrlPresign(x.Voucher.QrCodeImageUrl.Value)
                    : string.Empty,
                Place = x.Voucher.Place != null ? x.Voucher.Place.Value : null,
                LimitQuantity = x.Voucher.LimitQuantity,
                PartnerName = x.Voucher.Partner != null ? x.Voucher.Partner.PartnerName.Value : string.Empty,
                DiscountValue = x.Voucher.DiscountValue,
                DiscountPercent = x.Voucher.DiscountPercent,
                MaxDiscountValue = x.Voucher.MaxDiscountValue,
                MinOrderValue = x.Voucher.MinOrderValue
            }).ToList();
            return Result.Success(
                new PagedList<MemberVoucherResponse>(memberVouchers, result.TotalCount, result.CurrentPage,
                    request.PageSize));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}