using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Members.Responses;
using Domain.Abstractions;
using Domain.Members;
using Domain.Orders;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Application.Members.GetOne;

public class GetMemberByIdCommandHandler : ICommandHandler<GetMemberByIdCommand, MemberResponse>
{
    private readonly IAwsS3Service _awsS3Service;
    private readonly IMemberRepository _memberRepository;
    private readonly IOrderRepository _orderRepository;

    public GetMemberByIdCommandHandler(IMemberRepository memberRepository, IAwsS3Service awsS3Service,
        IOrderRepository orderRepository)
    {
        _memberRepository = memberRepository;
        _awsS3Service = awsS3Service;
        _orderRepository = orderRepository;
    }

    public async Task<Result<MemberResponse>> Handle(GetMemberByIdCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetEntitiesAsQueryable()
            .Include(x => x.MembershipClass)
            .Include(x => x.MemberPointHistories)
            .Include(x => x.District)
            .Include(x => x.MemberVouchers)
            .ThenInclude(x => x.Voucher)
            .FirstOrDefaultAsync(m => m.Id.Equals(request.MemberId), cancellationToken);

        if (member is null)
            return Result.Failure<MemberResponse>(MemberErrors.NotFound);
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
            DistrictId = member?.DistrictId?.Value
        };
        return Result.Success(resultResponse);
    }
}