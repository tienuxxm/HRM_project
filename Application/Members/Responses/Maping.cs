using Application.Abstractions.AWS;
using Application.Vouchers.GetOne;
using Domain.Members;

namespace Application.Members.Responses;

public static class Maping
{
    public static List<MemberResponse> MapToResponseList(this IEnumerable<Member> value, IAwsS3Service awsS3Service)
    {
        return value.Select(member => member.MemberResponse(awsS3Service)).ToList();
    }

    public static MemberResponse MemberResponse(this Member member, IAwsS3Service awsS3Service)
    {
        return new MemberResponse()
        {
            Email = member.Email.Value,
            Id = member.Id.Value,
            FirstName = member.FirstName.Value,
            LastName = member.LastName.Value,
            Address = member.Address.Value,
            PhoneNumber = member.PhoneNumber.Value,
            MemberCode = member.MemberCode.Value,
            BirthDate = member.BirthDate,
            AvatarUrl = member.Avatar != null ? awsS3Service.GetUrlPresign(member.Avatar.Value) : "",
            MembershipClass = member.MembershipClass?.ClassName.Value,
            MoneyForNextClass = member.MembershipClass?.MaxMoney.Amount,
            Currency = member.MembershipClass?.MaxMoney.Currency.Code,
            MemberPoint = member?.MemberPointHistories?
                .Sum(x => x.MemberPoint.Value)
        };
    }
}