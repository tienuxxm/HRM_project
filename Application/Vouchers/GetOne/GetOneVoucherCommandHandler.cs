using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.MemberVouchers;
using Domain.Partners;
using Domain.Vouchers;
using Microsoft.EntityFrameworkCore;

namespace Application.Vouchers.GetOne;

internal sealed class GetOneVoucherCommandHandler : ICommandHandler<GetOneVoucherCommand, VoucherResponse>
{
    private readonly IAwsS3Service _awsS3Service;
    private readonly IMemberVoucherRepository _memberVoucherRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVoucherRepository _voucherRepository;

    public GetOneVoucherCommandHandler(
        IVoucherRepository voucherRepository,
        IUnitOfWork unitOfWork, IAwsS3Service awsS3Service, IMemberVoucherRepository memberVoucherRepository)
    {
        _voucherRepository = voucherRepository;
        _unitOfWork = unitOfWork;
        _awsS3Service = awsS3Service;
        _memberVoucherRepository = memberVoucherRepository;
    }

    public async Task<Result<VoucherResponse>> Handle(GetOneVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await _voucherRepository.GetByIdAsync(request.VoucherId, cancellationToken);
        if (voucher is null) return Result.Failure<VoucherResponse>(PartnerErrors.NotFound);

        var voucherResponse = new VoucherResponse
        {
            Id = voucher.Id.Value,
            ContentVoucher = voucher.ContentVoucher?.Value,
            CreatedDate = voucher.CreatedDate,
            StartedDate = voucher.StartedDate,
            EndedDate = voucher.EndedDate,
            ImageUrl = _awsS3Service.GetUrlPresign(voucher.ImageUrl.Value),
            ImageId = voucher.ImageUrl.Value,
            IsVoucherDefault = voucher.IsVoucherDefault,
            Status = voucher.Status,
            TitleVoucher = voucher.TitleVoucher.Value,
            Point = voucher.Point,
            QrCode = voucher.QrCode?.Value,
            QrCodeImageUrl = voucher.QrCodeImageUrl?.Value != null
                ? _awsS3Service.GetUrlPresign(voucher.QrCodeImageUrl.Value)
                : null,
            QrCodeImageId = voucher.QrCodeImageUrl?.Value,
            Place = voucher.Place?.Value,
            PartnerId = voucher.PartnerId?.Value,
            PartnerName = voucher.Partner?.PartnerName.Value,
            Conditions = voucher.Conditions?.Value,
            LimitQuantity = voucher.LimitQuantity.HasValue ? voucher.LimitQuantity.Value : null,
            DiscountValue = voucher.DiscountValue,
            DiscountPercent = voucher.DiscountPercent,
            MaxDiscountValue = voucher.MaxDiscountValue,
            IsUserVoucher = voucher.IsUserVoucher,
            MinOrderValue = voucher.MinOrderValue,
            Index = voucher.Index,
            Members = voucher.Members,
            Memberships = voucher.Memberships
        };
        if (!voucher.IsUserVoucher.HasValue || !voucher.IsUserVoucher.Value) return voucherResponse;
        var memberCode = (await _memberVoucherRepository.GetEntitiesAsQueryable().Include(x => x.Member)
            .FirstOrDefaultAsync(x => x.VoucherId == voucher.Id, cancellationToken))?.Member?.MemberCode?.Value;
        voucherResponse.MemberCode = memberCode;

        return voucherResponse;
    }
}