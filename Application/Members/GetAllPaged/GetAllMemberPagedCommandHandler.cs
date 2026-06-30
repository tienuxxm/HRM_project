using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Members.Responses;
using Domain.Abstractions;
using Domain.Members;

namespace Application.Members.GetAllPaged;

public class GetAllMemberPagedCommandHandler : ICommandHandler<GetAllMemberPagedCommand, PagedList<MemberResponse>>
{
    private readonly IAwsS3Service _awsS3Service;
    private readonly IMemberRepository _memberRepository;

    public GetAllMemberPagedCommandHandler(IMemberRepository memberRepository, IAwsS3Service awsS3Service)
    {
        _memberRepository = memberRepository;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<PagedList<MemberResponse>>> Handle(GetAllMemberPagedCommand request,
        CancellationToken cancellationToken)
    {
        var query = _memberRepository.GetEntitiesAsQueryable()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.CreatedAt).AsQueryable();
        if (!string.IsNullOrEmpty(request.SearchTerm))
            query = query.AsEnumerable()
                .Where(x => (x.IsActive && x.Email.Value.ToLower().Contains(request.SearchTerm.ToLower()))
                            || x.FullName.ToLower().Contains(request.SearchTerm.ToLower()) ||
                            x.PhoneNumber.Value.Contains(request.SearchTerm) ||
                            x.MemberCode.Value.ToLower().Contains(request.SearchTerm.ToLower()))
                .AsQueryable();

        if (request.SortColumn == nameof(Member.FullName))
        {
            request.SortColumn = "";
            if (request.SortOrder == "ASC")
                query = query.AsEnumerable()
                    .OrderBy(x => x.FullName).AsQueryable();
            else
                query = query.AsEnumerable()
                    .OrderByDescending(x => x.FullName).AsQueryable();
        }

        var result = await _memberRepository.GetAllPaged(request, query);
        var memberResponses = result.Data.Select(member => new MemberResponse
        {
            Email = member.Email.Value,
            Id = member.Id.Value,
            FirstName = member.FirstName.Value,
            LastName = member.LastName.Value,
            Address = member.Address.Value,
            PhoneNumber = member.PhoneNumber.Value,
            MemberCode = member.MemberCode.Value,
            AvatarUrl = member.Avatar != null ? _awsS3Service.GetUrlPresign(member.Avatar.Value) : "",
            MembershipClass = member.MembershipClass?.ClassName.Value,
            MoneyForNextClass = member.MembershipClass?.MaxMoney.Amount,
            Currency = member.MembershipClass?.MaxMoney.Currency.Code,
            BirthDate = member.BirthDate,
            Note = member?.Note
        }).ToList();

        return Result.Success(new PagedList<MemberResponse>(memberResponses, result.TotalCount, result.CurrentPage,
            result.PageSize));
    }
}