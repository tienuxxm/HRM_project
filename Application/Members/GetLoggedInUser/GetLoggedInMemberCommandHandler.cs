using Application.Abstractions.Authentication;
using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Members.Responses;
using Domain.Abstractions;
using Domain.Members;
using Domain.Orders;
using Domain.QrCode;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Application.Members.GetLoggedInUser;

public class GetLoggedInMemberCommandHandler : ICommandHandler<GetLoggedInMemberCommand, MemberResponse>
{
    private readonly IAwsS3Service _awsS3Service;
    private readonly IMemberContext _memberContext;
    private readonly IMemberRepository _memberRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IQrCodeRepository _qrCodeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GetLoggedInMemberCommandHandler(IMemberContext memberContext, IMemberRepository memberRepository,
        IAwsS3Service awsS3Service, IOrderRepository orderRepository, IQrCodeRepository qrCodeRepository,
        IUnitOfWork unitOfWork)
    {
        _memberContext = memberContext;
        _memberRepository = memberRepository;
        _awsS3Service = awsS3Service;
        _orderRepository = orderRepository;
        _qrCodeRepository = qrCodeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<MemberResponse>> Handle(GetLoggedInMemberCommand request,
        CancellationToken cancellationToken)
    {
        var id = _memberContext.IdentityId;
        var member = await _memberRepository.GetEntitiesAsQueryable()
            .Include(x => x.MembershipClass)
            .Include(x => x.MemberPointHistories)
            .Include(x => x.MemberVouchers)
            .ThenInclude(x => x.Voucher)
            .Include(x => x.District)
            .ThenInclude(x => x.Province)
            .FirstOrDefaultAsync(m => m.IdentityId == id && m.IsActive, cancellationToken);

        if (member is null)
            return Result.Failure<MemberResponse>(MemberErrors.InvalidCredentials);

        var ordersTotalBill = await _orderRepository.GetEntitiesAsQueryable()
            .AsNoTracking()
            .Where(x => x.MemberId.Equals(member.Id) && x.HasPayment)
            .Select(x => x.TotalBill)
            .ToListAsync(cancellationToken);

        var totalPaid = ordersTotalBill.Count > 0
            ? ordersTotalBill.Aggregate((x, y) => x + y)
            : Money.Zero(Currency.Vnd);


        var resultResponse = new MemberResponse
        {
            Email = member.Email.Value,
            Id = member.Id.Value,
            FirstName = member.FirstName.Value,
            LastName = member.LastName.Value,
            Address = member.Address.Value,
            PhoneNumber = member.PhoneNumber.Value,
            MemberCode = member.MemberCode.Value,
            BirthDate = member.BirthDate,
            AvatarUrl = member.Avatar != null ? _awsS3Service.GetUrlPresign(member.Avatar.Value) : "",
            MembershipClass = member.MembershipClass?.ClassName.Value,
            MoneyForNextClass = member.MembershipClass?.MaxMoney.Amount,
            Currency = member.MembershipClass?.MaxMoney.Currency.Code,
            MemberPoint = member?.MemberPointHistories?.Sum(x => x.MemberPoint.Value),
            TotalValidVoucher = member?.MemberVouchers.Count(x => !x.IsUsed) ?? 0,
            TotalPaid = totalPaid,
            DistrictId = member?.DistrictId?.Value,
            ProvinceId = member?.District?.Province.Id.Value,
            MembershipAssignedDate = member?.MembershipAssignedDate
        };

        Guid? memberQrCode = null;

        try
        {
            var qrCode = await _qrCodeRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(x => x.LinkId.Equals(new QrCodeLinkId(resultResponse.Id)), cancellationToken);
            if (qrCode is null)
            {
                var newQrCode = Domain.QrCode.QrCode.Create(new QrCodeLinkId(resultResponse.Id), QrCodeType.MEMBER);
                memberQrCode = newQrCode.Id.Value;
                _qrCodeRepository.Add(newQrCode);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                memberQrCode = qrCode.Id.Value;
            }
        }
        catch (Exception)
        {
            //Ignore
        }

        resultResponse.QrCodeId = memberQrCode.HasValue ? memberQrCode.Value : null;

        return resultResponse;
    }
}