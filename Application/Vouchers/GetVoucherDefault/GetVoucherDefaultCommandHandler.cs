using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Vouchers.GetOne;
using Domain.Abstractions;
using Domain.Vouchers;
using Microsoft.EntityFrameworkCore;

namespace Application.Vouchers.GetVoucherDefault;

public class GetVoucherDefaultCommandHandler : ICommandHandler<GetVoucherDefaultCommand, VoucherResponse>
{
    private readonly IVoucherRepository _voucherRepository;
    private readonly IAwsS3Service _awsS3Service;

    public GetVoucherDefaultCommandHandler(IVoucherRepository voucherRepository, IAwsS3Service awsS3Service)
    {
        _voucherRepository = voucherRepository;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<VoucherResponse>> Handle(GetVoucherDefaultCommand request,
        CancellationToken cancellationToken)
    {
        var voucher = await _voucherRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(v => v.IsActive
                                      && v.IsVoucherDefault.HasValue
                                      && v.IsVoucherDefault.Value && v.VoucherDefaultType.HasValue &&
                                      v.VoucherDefaultType == request.VoucherDefaultType &&
                                      (!v.IsUserVoucher.HasValue || !v.IsUserVoucher.Value),
                cancellationToken);
        if (voucher is null)
        {
            return Result.Failure<VoucherResponse>(VoucherErrors.NotFound);
        }

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
            MinOrderValue = voucher.MinOrderValue,
            Index = voucher.Index
        };
        return voucherResponse;
    }
}