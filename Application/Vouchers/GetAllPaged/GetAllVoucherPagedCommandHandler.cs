using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Vouchers.GetOne;
using Domain.Abstractions;
using Domain.MemberVouchers;
using Domain.Partners;
using Domain.Vouchers;
using Microsoft.EntityFrameworkCore;

namespace Application.Vouchers.GetAllPaged;

public class GetAllVoucherPagedCommandHandler : ICommandHandler<GetAllVoucherPagedCommand, GetAllVoucherPagedResposne>
{
    private readonly IAwsS3Service _awsS3Service;
    private readonly IMemberVoucherRepository _memberVoucherRepository;
    private readonly IVoucherRepository _voucherRepository;

    public GetAllVoucherPagedCommandHandler(IVoucherRepository voucherRepository, IAwsS3Service awsS3Service,
        IMemberVoucherRepository memberVoucherRepository)
    {
        _voucherRepository = voucherRepository;
        _awsS3Service = awsS3Service;
        _memberVoucherRepository = memberVoucherRepository;
    }

    public async Task<Result<GetAllVoucherPagedResposne>> Handle(GetAllVoucherPagedCommand request,
        CancellationToken cancellationToken)
    {
        var query = _voucherRepository.GetEntitiesAsQueryable()
            .Include(x => x.Partner)
            .Where(x =>
                !x.IsVoucherDefault.HasValue || !x.IsVoucherDefault.Value)
            .AsQueryable();
        if (request.PartnerId.HasValue)
            query = query.Where(x => x.PartnerId != null && x.PartnerId.Equals(new PartnerId(request.PartnerId.Value)));
        else
            query = query.Where(x => x.PartnerId == null);

        if (!string.IsNullOrEmpty(request.SearchTerm))
            query = query.AsEnumerable()
                .Where(x => x.TitleVoucher.Value.ToLower().Contains(request.SearchTerm.ToLower())
                            || x.CreatedDate.ToString("dd/MM/yyyy").Contains(request.SearchTerm)
                            || x.EndedDate.ToString("dd/MM/yyyy").Contains(request.SearchTerm)
                            || x.Point.ToString().Contains(request.SearchTerm))
                .AsQueryable();

        if (request.SortColumn is "Date")
        {
            query = request.SortOrder == "ASC"
                ? query.OrderBy(x => x.CreatedDate)
                    .ThenBy(x => x.EndedDate)
                : query.OrderByDescending(x => x.StartedDate)
                    .ThenByDescending(x => x.EndedDate);
            request.SortOrder = "";
        }

        var result = await _voucherRepository.GetAllPaged(request, query);
        var voucherDtos = result.Data.Select(x => new VoucherResponse
        {
            Id = x.Id.Value,
            Point = x.Point,
            StartedDate = x.StartedDate,
            EndedDate = x.EndedDate,
            TitleVoucher = x.TitleVoucher.Value,
            ImageUrl = _awsS3Service.GetUrlPresign(x.ImageUrl.Value),
            ContentVoucher = x.ContentVoucher?.Value,
            CreatedDate = x.CreatedDate.Date,
            QrCode = x.QrCode?.Value,
            QrCodeImageUrl = _awsS3Service.GetUrlPresign(x.QrCodeImageUrl?.Value ?? ""),
            Conditions = x.Conditions?.Value,
            PartnerId = x.PartnerId?.Value,
            QrCodeId = x.QrCodeId,
            IsUserVoucher = x.IsUserVoucher
        }).ToList();
        var userVouchers = voucherDtos.Where(x => x.IsUserVoucher.HasValue && x.IsUserVoucher.Value)
            .Select(x => new VoucherId(x.Id))
            .ToList();

        var memberVouchers = await _memberVoucherRepository
            .GetEntitiesAsQueryable()
            .Include(x => x.Member)
            .Where(x => userVouchers.Any(k => k == x.VoucherId))
            .Select(x => new { x.VoucherId, x.Member.MemberCode })
            .ToListAsync(cancellationToken);
        memberVouchers.ForEach(mv =>
        {
            var fdVoucher = voucherDtos.First(x => x.Id == mv.VoucherId.Value);
            fdVoucher.MemberCode = mv.MemberCode.Value;
        });
        return Result.Success(new GetAllVoucherPagedResposne(voucherDtos, result.TotalCount,
            result.CurrentPage, result.PageSize));
    }
}