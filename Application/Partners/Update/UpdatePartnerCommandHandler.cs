using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Partners;
using Domain.Products;
using Domain.QrCode;
using Domain.Shared;
using Domain.Vouchers;

namespace Application.Partners.Update;

internal sealed class UpdatePartnerCommandHandler : ICommandHandler<UpdatePartnerCommand, Partner>
{
    private readonly IPartnerRepository _partnerRepository;
    private readonly IVoucherRepository _voucherRepository;
    private readonly IQrCodeRepository _qrCodeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private List<Voucher>? _vouchers;
    private PartnerName? _partnerName;
    private PartnerAddress? _partnerAddress;
    private PhoneNumber? _phoneNumber;
    private Email? _email;

    public UpdatePartnerCommandHandler(IPartnerRepository partnerRepository, IUnitOfWork unitOfWork,
        IVoucherRepository voucherRepository, IQrCodeRepository qrCodeRepository)
    {
        _partnerRepository = partnerRepository;
        _unitOfWork = unitOfWork;
        _voucherRepository = voucherRepository;
        _qrCodeRepository = qrCodeRepository;
    }

    public async Task<Result<Partner>> Handle(UpdatePartnerCommand request, CancellationToken cancellationToken)
    {
        var partner = await _partnerRepository.GetByIdAsync(request.PartnerId);
        if (partner is null)
        {
            return Result.Failure<Partner>(ProductErrors.NotFound);
        }

        if (request.VoucherIds != null)
        {
            _vouchers = await _voucherRepository.GetAll();
        }

        if (request.Name != null)
        {
            _partnerName = new PartnerName(request.Name);
        }

        if (request.address != null)
        {
            _partnerAddress = new PartnerAddress(request.address);
        }

        if (request.phoneNumber != null)
        {
            _phoneNumber = new PhoneNumber(request.phoneNumber);
        }

        if (request.email != null)
        {
            _email = new Email(request.email);
        }

        if (string.IsNullOrEmpty(partner.QrCodeId))
        {
            var qrCodeId = Domain.QrCode.QrCode.Create(new QrCodeLinkId(partner.Id.Value), QrCodeType.PARTNER);
            _qrCodeRepository.Add(qrCodeId);
            partner.SetQrCodeId(qrCodeId.Id.Value.ToString());
        }


        partner.Update(
            _partnerName,
            _partnerAddress,
            _phoneNumber,
            _email,
            _vouchers
        );

        _partnerRepository.Update(partner);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return partner;
    }
}