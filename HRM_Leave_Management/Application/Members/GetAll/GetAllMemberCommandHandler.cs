using Application.Abstractions.Messaging;
using Application.Members.Responses;
using Domain.Abstractions;
using Domain.Members;

namespace Application.Members.GetAll;

public class GetAllMemberCommandHandler : ICommandHandler<GetAllMemberCommand, List<MemberResponse>>
{
    private readonly IMemberRepository _memberRepository;

    public GetAllMemberCommandHandler(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<Result<List<MemberResponse>>> Handle(GetAllMemberCommand request,
        CancellationToken cancellationToken)
    {
        var members = await _memberRepository.GetAll(cancellationToken);
        if (members is null)
            return Result.Success(new List<MemberResponse>());
        var membersResponse = members.Select(x => new MemberResponse()
        {
            Id = x.Id.Value,
            Address = x.Address.Value,
            Email = x.Email.Value,
            FirstName = x.FirstName.Value,
            LastName = x.LastName.Value,
            PhoneNumber = x.PhoneNumber.Value,
            BirthDate = x.BirthDate
        }).ToList();
        return Result.Success(membersResponse);
    }
}