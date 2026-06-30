using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.Members;

namespace Application.Members.IsPhoneNumberValid;

internal sealed class IsPhoneNumberValidCommandHandler : ICommandHandler<IsPhoneNumberValidCommand, BooleanResponse>
{
    private readonly IMemberRepository _memberRepository;

    public IsPhoneNumberValidCommandHandler(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<Result<BooleanResponse>> Handle(IsPhoneNumberValidCommand request,
        CancellationToken cancellationToken)
    {
        var isPhoneExisted =
            await _memberRepository.IsExistedAsync(
                x => !string.IsNullOrEmpty(x.IdentityId) && x.IsActive && x.PhoneNumber.Equals(request.PhoneNumber),
                cancellationToken);
        return Result.Success(new BooleanResponse { Result = isPhoneExisted });
    }
}