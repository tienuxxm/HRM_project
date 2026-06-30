using Application.Abstractions.Messaging;
using Application.Members.Responses;
using Domain.Abstractions;
using Domain.Members;
using Domain.Restaurants;

namespace Application.Members.Search;

public class MemberSearchCommandHandler : ICommandHandler<MemberSearchCommand, List<MemberResponse>>
{
    private readonly IMemberRepository _memberRepository;

    public MemberSearchCommandHandler(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<Result<List<MemberResponse>>> Handle(MemberSearchCommand request,
        CancellationToken cancellationToken)
    {
        var members = await _memberRepository.Search(request.SearchValue, cancellationToken);
        var memberResponse = members?.Select(x => new MemberResponse()
        {
            Id = x.Id.Value,
            Address = x.Address.Value,
            Email = x.Email.Value,
            FirstName = x.FirstName.Value,
            LastName = x.LastName.Value,
            PhoneNumber = x.PhoneNumber.Value,
            BirthDate = x.BirthDate
        }).ToList();
        return memberResponse;
    }
}