using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Vouchers.GetOne;
using Domain.Abstractions;
using Domain.Vouchers;

namespace Application.Vouchers.GetAll;

internal sealed class GetAllVoucherCommandHandler : ICommandHandler<GetAllVoucherCommand, List<VoucherResponse>>
{
    private readonly IVoucherRepository _voucherRepository;
    private readonly IAwsS3Service _awsS3Service;

    public GetAllVoucherCommandHandler(
        IVoucherRepository voucherRepository, IAwsS3Service awsS3Service)
    {
        _voucherRepository = voucherRepository;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<List<VoucherResponse>>> Handle(GetAllVoucherCommand request,
        CancellationToken cancellationToken)
    {
        var voucher = await _voucherRepository.GetAll(cancellationToken);
        if (voucher is null)
            return Result.Failure<List<VoucherResponse>>(VoucherErrors.NotFound);
        var voucherResponse = voucher?.Select(x => new VoucherResponse()
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
            QrCodeImageUrl = x.QrCodeImageUrl?.Value,
            Place = x.Place?.Value
        }).ToList();
        return voucherResponse;
    }
}