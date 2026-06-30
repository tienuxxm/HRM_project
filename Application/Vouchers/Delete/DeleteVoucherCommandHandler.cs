using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Images;
using Domain.Partners;
using Domain.Vouchers;

namespace Application.Vouchers.Delete;

internal sealed class DeleteVoucherCommandHandler : ICommandHandler<DeleteVoucherCommand>
{
    private readonly IVoucherRepository _voucherRepository;
    private readonly IImageRepository _imageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteVoucherCommandHandler(
        IVoucherRepository voucherRepository,
        IImageRepository imageRepository,
        IUnitOfWork unitOfWork)
    {
        _voucherRepository = voucherRepository;
        _imageRepository = imageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteVoucherCommand request, CancellationToken cancellationToken)
    {
        var partner = await _voucherRepository.GetByIdAsync(request.VoucherId, cancellationToken);
        if (partner is null)
        {
            return Result.Failure<Guid>(PartnerErrors.NotFound);
        }

        _voucherRepository.Remove(partner);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}