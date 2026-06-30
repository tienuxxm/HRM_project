using Application.Abstractions.Messaging;
using Application.Members.GetOne;
using Application.Members.Responses;
using Domain.Abstractions;
using Domain.Members;
using MediatR;

namespace Application.Members.GetByPhone;

public class GetMemberInfoByPhoneCommandHandler : ICommandHandler<GetMemberInfoByPhoneCommand, MemberResponse>
{
    private readonly IMemberRepository _memberRepository;
    private readonly ISender _sender;

    public GetMemberInfoByPhoneCommandHandler(ISender sender, IMemberRepository memberRepository)
    {
        _sender = sender;
        _memberRepository = memberRepository;
    }

    public async Task<Result<MemberResponse>> Handle(GetMemberInfoByPhoneCommand request,
        CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByPhoneNumberAsync(request.PhoneNumber, cancellationToken);
        if (member is null)
            return Result.Failure<MemberResponse>(new Error("Member.NotFound", "Không tìm thấy Customer"));

        var command = new GetMemberByIdCommand(member.Id);
        return await _sender.Send(command, cancellationToken);
    }
}