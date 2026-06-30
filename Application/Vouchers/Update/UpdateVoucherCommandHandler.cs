using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Images;
using Domain.Partners;
using Domain.Products;
using Domain.QrCode;
using Domain.Shared;
using Domain.Vouchers;

namespace Application.Vouchers.Update;

internal class UpdateVoucherCommandHandler : ICommandHandler<UpdateVoucherCommand, Voucher>
{
    private readonly IVoucherRepository _voucherRepository;
    private readonly IQrCodeRepository _qrCodeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateVoucherCommandHandler(
        IVoucherRepository voucherRepository,
        IUnitOfWork unitOfWork, IQrCodeRepository qrCodeRepository)
    {
        _unitOfWork = unitOfWork;
        _qrCodeRepository = qrCodeRepository;
        _voucherRepository = voucherRepository;
    }

    public async Task<Result<Voucher>> Handle(UpdateVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await _voucherRepository.GetByIdAsync(request.VoucherId, cancellationToken);
        if (voucher is null)
        {
            return Result.Failure<Voucher>(VoucherErrors.NotFound);
        }

        voucher.Update(request.TitleVoucher, request.ImageUrl, request.StartedDate, request.EndedDate, request.Place,
            request.Point, request.PartnerId, request.ContentVoucher, request.Conditions,
            request.LimitQuantity, request.DiscountValue, request.DiscountPercent, request.MaxDiscountValue,
            request.MinOrderValue, request.Index, request.VoucherDefaultType);

        if (string.IsNullOrEmpty(voucher.QrCodeId))
        {
            var qrCodeId = Domain.QrCode.QrCode.Create(new QrCodeLinkId(voucher.Id.Value), QrCodeType.VOUCHER);
            _qrCodeRepository.Add(qrCodeId);
            voucher.SetQrCodeId(qrCodeId.Id.Value.ToString());
        }

        _voucherRepository.Update(voucher);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return voucher;
    }
}