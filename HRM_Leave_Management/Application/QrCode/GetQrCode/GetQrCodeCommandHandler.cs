using System.Globalization;
using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Extensions;
using Application.Vouchers.GetOne;
using Domain.Abstractions;
using Domain.Members;
using Domain.Orders;
using Domain.Partners;
using Domain.QrCode;
using Domain.Shared;
using Domain.Vouchers;
using Microsoft.EntityFrameworkCore;

namespace Application.QrCode.GetQrCode;

public class GetQrCodeCommandHandler : ICommandHandler<GetQrCodeCommand, GetQrCodeResponse>
{
    private readonly IQrCodeRepository _qrCodeRepository;
    private readonly IPartnerRepository _partnerRepository;
    private readonly IVoucherRepository _voucherRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IAwsS3Service _awsS3Service;
    private readonly IOrderRepository _orderRepository;

    public GetQrCodeCommandHandler(IQrCodeRepository qrCodeRepository, IPartnerRepository partnerRepository,
        IVoucherRepository voucherRepository, IAwsS3Service awsS3Service, IMemberRepository memberRepository,
        IOrderRepository orderRepository)
    {
        _qrCodeRepository = qrCodeRepository;
        _partnerRepository = partnerRepository;
        _voucherRepository = voucherRepository;
        _awsS3Service = awsS3Service;
        _memberRepository = memberRepository;
        _orderRepository = orderRepository;
    }

    public async Task<Result<GetQrCodeResponse>> Handle(GetQrCodeCommand request, CancellationToken cancellationToken)
    {
        var invalidQrcodeResult = Result.Failure<GetQrCodeResponse>(new Error("QrCode.NotFound", "Invalid QrCode"));
        var qrCode = await _qrCodeRepository.GetEntitiesAsQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id.Equals(request.Id), cancellationToken);
        if (qrCode is null)
            return invalidQrcodeResult;

        switch (qrCode.Type)
        {
            case QrCodeType.PARTNER:
                var partner = await _partnerRepository.GetEntitiesAsQueryable()
                    .FirstOrDefaultAsync(x => x.QrCodeId == qrCode.Id.Value.ToString(), cancellationToken);
                if (partner is null)
                    return invalidQrcodeResult;
                var vouchers = await _voucherRepository.GetEntitiesAsQueryable()
                    .AsNoTracking()
                    .Include(x => x.Partner)
                    .Where(v => v.PartnerId != null && v.PartnerId.Equals(partner.Id))
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
                        Place = x.Place != null ? x.Place.Value : null,
                        Status = x.Status,
                        LimitQuantity = x.LimitQuantity,
                        PartnerName = x.Partner != null ? x.Partner.PartnerName.Value : String.Empty,
                        PartnerId = x.PartnerId != null ? x.PartnerId.Value : null
                    })
                    .ToListAsync(cancellationToken);
                return Result.Success(new GetQrCodeResponse()
                {
                    Type = "VOUCHER",
                    Data = vouchers
                });
            case QrCodeType.VOUCHER:
                var voucher = await _voucherRepository.GetEntitiesAsQueryable()
                    .Include(x => x.Partner)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id.Equals(new VoucherId(qrCode.LinkId.Value)), cancellationToken);
                if (voucher is null)
                    return invalidQrcodeResult;
                return Result.Success(new GetQrCodeResponse()
                {
                    Type = "VOUCHER",
                    Data = new
                    {
                        Id = voucher.Id.Value,
                        Conditions = voucher.Conditions?.Value,
                        Partner = voucher.Partner?.PartnerName.Value,
                        Place = voucher.Place?.Value,
                        voucher.Status,
                        Title = voucher.TitleVoucher.Value,
                        voucher.Point,
                        voucher.CreatedDate,
                        voucher.IsExpired,
                        voucher.LimitQuantity,
                        voucher.QrCodeId,
                        StartedDate = voucher.StartedDate.ToCustomDateFormat(),
                        EndedDate = voucher.EndedDate.ToCustomDateFormat()
                    }
                });
            case QrCodeType.MEMBER:
                var member = await _memberRepository.GetEntitiesAsQueryable()
                    .AsNoTracking()
                    .Include(x => x.MemberVouchers)
                    .ThenInclude(x => x.Voucher)
                    .ThenInclude(x => x.Partner)
                    .Include(x => x.MemberPointHistories)
                    .Include(x => x.MembershipClass)
                    .Include(m => m.District)
                    .ThenInclude(d => d.Province)
                    .FirstOrDefaultAsync(x => x.Id.Equals(new MemberId(qrCode.LinkId.Value)), cancellationToken);
                if (member is null)
                    return invalidQrcodeResult;
                var ordersTotalBill = await _orderRepository.GetEntitiesAsQueryable()
                    .AsNoTracking()
                    .Where(x => x.MemberId.Equals(member.Id) && x.HasPayment)
                    .Select(x => x.TotalBill)
                    .ToListAsync(cancellationToken);
                var orderHistories = await _orderRepository.GetEntitiesAsQueryable()
                    .AsNoTracking()
                    .Include(x => x.LineItems)
                    .Include(x => x.Delivery)
                    .Include(x => x.OrderFees)
                    .OrderByDescending(x => x.CreatedDate)
                    .Where(x => x.MemberId.Equals(member.Id))
                    .Select(order => new
                    {
                        Id = order.Id.Value,
                        Note = order.Note == null ? null : order.Note.Value,
                        order.Status,
                        order.CompletedDate,
                        order.CreatedDate,
                        MemberId = order.MemberId.Value,
                        OrderCode = order.OrderCode.Value,
                        order.HasPayment,
                        order.PaymentType,
                        TotalPrice = order.TotalBill,
                        TotalQuantity = order.LineItems.Count > 0 ? order.LineItems.Sum(x => x.Quantity) : 0,
                        Delivery = order.Delivery != null
                            ? new
                            {
                                Note = order.Delivery.Note.Value,
                                CompanyAddress = order.Delivery.CompanyAddress.Value,
                                CompanyEmail = order.Delivery.CompanyEmail.Value,
                                CompanyName = order.Delivery.CompanyName.Value,
                                CompanyTaxCode = order.Delivery.CompanyTaxCode.Value,
                                PhoneNumber = order.Delivery.PhoneNumber.Value,
                                ReceiverName = order.Delivery.ReceiverName.Value,
                                ReceivingAddress = order.Delivery.ReceivingAddress.Value,
                                HasRequestCutlery = order.Delivery.HasRequestCutlery.Value,
                                HasIssueAnInvoice = order.Delivery.HasIssueAnInvoice.Value,
                            }
                            : null,
                        OrderFees = order.OrderFees.Select(fee => new
                        {
                            ChargeFee = fee.OrderFeeCharge,
                            FeeName = fee.OrderFeeName.Value,
                            FeeValue = fee.OrderFeeValue.Value,
                            fee.IsPercent
                        }).ToList(),
                        LineItems = order.LineItems.Select(l => new
                        {
                            Id = l.Id.Value,
                            l.Price,
                            l.Quantity,
                            ProductId = l.ProductId!.Value,
                            OrderId = l.OrderId.Value,
                            ProductName = l.ProductName.Value
                        }).ToList()
                    })
                    .ToListAsync(cancellationToken);

                var totalPaid = ordersTotalBill.Count > 0
                    ? ordersTotalBill.Aggregate((x, y) => x + y)
                    : Money.Zero(Currency.Vnd);
                return Result.Success(new GetQrCodeResponse()
                {
                    Type = "MEMBER",
                    Data = new
                    {
                        Email = member.Email.Value,
                        Id = member.Id.Value,
                        FirstName = member.FirstName.Value,
                        LastName = member.LastName.Value,
                        Address = member.Address.Value,
                        PhoneNumber = member.PhoneNumber.Value,
                        MemberCode = member.MemberCode.Value,
                        BirthDate = member.BirthDate.HasValue ? null : member.BirthDate?.ToString("dd/MM/yyyy"),
                        AvatarUrl = member.Avatar != null ? _awsS3Service.GetUrlPresign(member.Avatar.Value) : "",
                        MembershipClass = member.MembershipClass?.ClassName.Value,
                        MoneyForNextClass = member.MembershipClass?.MaxMoney.Amount.ToString("#,###",
                                                CultureInfo.GetCultureInfo("vi-VN").NumberFormat) + " " +
                                            member.MembershipClass?.MaxMoney.Currency.Code,
                        MemberPoint = member?.MemberPointHistories?.Sum(x => x.MemberPoint.Value),
                        TotalValidVoucher = member?.MemberVouchers.Count(x => !x.IsUsed) ?? 0,
                        UsedVouchers = member?.MemberVouchers.Where(x => x.IsUsed).Select(v => new
                        {
                            Id = v.Voucher.Id.Value,
                            VoucherName = v.Voucher.TitleVoucher.Value,
                            Content = v.Voucher.ContentVoucher?.Value,
                            Place = v.Voucher.Place?.Value,
                            v.IsVoucherExpired,
                            Conditions = v.Voucher.Conditions?.Value,
                            v.Voucher.Point,
                            v.Voucher.CreatedDate,
                            v.Voucher.StartedDate,
                            v.Voucher.EndedDate,
                        }).ToList(),
                        UnusedVouchers = member?.MemberVouchers.Where(x => !x.IsUsed).Select(v => new
                        {
                            Id = v.Voucher.Id.Value,
                            VoucherName = v.Voucher.TitleVoucher.Value,
                            Content = v.Voucher.ContentVoucher?.Value,
                            Place = v.Voucher.Place?.Value,
                            v.IsVoucherExpired,
                            Conditions = v.Voucher.Conditions?.Value,
                            v.Voucher.Point,
                            CreatedDate = v.Voucher.CreatedDate.ToCustomDateFormat(),
                            StartedDate = v.Voucher.StartedDate.ToCustomDateFormat(),
                            v.Voucher.EndedDate,
                        }).ToList(),
                        TotalPaid = totalPaid.Amount.ToString("#,###",
                            CultureInfo.GetCultureInfo("vi-VN").NumberFormat) + " " + totalPaid.Currency.Code,
                        Province = member?.District?.Province.Name,
                        District = member?.District?.Name,
                        /*OrderHistories = orderHistories*/
                    }
                });

            default:
                return invalidQrcodeResult;
        }
    }
}