using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth.ResetPassword;

public class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand, bool>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IMemberRepository _memberRepository;

    public ResetPasswordCommandHandler(IAuthenticationService authenticationService, IMemberRepository memberRepository)
    {
        _authenticationService = authenticationService;
        _memberRepository = memberRepository;
    }

    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(x => x.PhoneNumber == new PhoneNumber(request.PhoneNumber) && x.IsActive,
                cancellationToken);
        if (member is null)
            return Result.Failure<bool>(MemberErrors.NotFound);
        if (string.IsNullOrEmpty(member.IdentityId))
            return Result.Failure<bool>(MemberErrors.NotFound);
        var resetPasswordResult =
            await _authenticationService.ResetPassword(request.newPassword, member.IdentityId, cancellationToken);
        return resetPasswordResult.IsSuccess;
    }
}