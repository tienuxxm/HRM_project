using Application.Abstractions.Messaging;
using Application.Vouchers.GetOne;
using Domain.Abstractions;
using Domain.Partners;

namespace Application.Partners.GetOne;

internal sealed class GetOnePartnerCommandHandler : IQueryHandler<GetOnePartnerCommand, PartnerResponse>
{
    private readonly IPartnerRepository _partnerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GetOnePartnerCommandHandler(
        IPartnerRepository partnerRepository,
        IUnitOfWork unitOfWork)
    {
        _partnerRepository = partnerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PartnerResponse>> Handle(GetOnePartnerCommand request, CancellationToken cancellationToken)
    {
        var partner = await _partnerRepository.GetByIdAsync(request.PartnerId);
        if (partner is null)
        {
            return Result.Failure<PartnerResponse>(PartnerErrors.NotFound);
        }

        var result = new PartnerResponse
        {
            Id = partner.Id.Value,
            PartnerName = partner.PartnerName.Value,
            Address = partner.Address?.Value,
            Email = partner.Email?.Value,
            PhoneNumber = partner.PhoneNumber?.Value,
            CreatedDate = partner.CreatedDate,
            Vouchers = new List<VoucherResponse>()
        };
        if (partner.Vouchers != null)
        {
            foreach (var voucher in partner.Vouchers)
            {
                var voucherResponse = new VoucherResponse
                {
                    Id = voucher.Id.Value,
                    ContentVoucher = voucher.ContentVoucher?.Value,
                    CreatedDate = voucher.CreatedDate,
                    StartedDate = voucher.StartedDate,
                    EndedDate = voucher.EndedDate,
                    ImageUrl = voucher.ImageUrl.Value,
                    IsVoucherDefault = voucher.IsVoucherDefault,
                    Status = voucher.Status,
                    TitleVoucher = voucher.TitleVoucher.Value,
                    Point = voucher.Point
                };
                result.Vouchers.Add(voucherResponse);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}