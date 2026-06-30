using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Members;

namespace Application.Members.ChangePassword;

public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
{
    private readonly IMemberContext _memberContext;
    private readonly IAuthenticationService _authenticationService;
    private readonly IMemberRepository _memberRepository;
    private readonly IJwtService _jwtService;

    public ChangePasswordCommandHandler(IMemberContext memberContext, IAuthenticationService authenticationService,
        IMemberRepository memberRepository, IJwtService jwtService)
    {
        _memberContext = memberContext;
        _authenticationService = authenticationService;
        _memberRepository = memberRepository;
        _jwtService = jwtService;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var memberId = _memberContext.IdentityId;
        var memberResult = await _memberRepository.GetByIdentityAsync(memberId, cancellationToken);
        if (memberResult is null)
        {
            return Result.Failure(MemberErrors.NotFound);
        }

        var result = await _jwtService.GetAccessTokenAsync(
            memberResult.Email.Value,
            request.CurrentPassword,
            cancellationToken);
        if (!result.IsSuccess)
            return Result.Failure(new Error("ChangePassword.Fail", "Mật khẩu hiện tại không chính xác"));

        var changePasswordResult =
            await _authenticationService.ResetPassword(request.NewPassword, memberId, cancellationToken);

        return changePasswordResult;
    }
}