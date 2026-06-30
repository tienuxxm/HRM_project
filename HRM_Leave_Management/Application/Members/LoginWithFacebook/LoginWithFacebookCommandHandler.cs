using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace Application.Members.LoginWithFacebook;

public class LoginWithFacebookCommandHandler : ICommandHandler<LoginWithFacebookCommand, TokenResponse>
{
    private readonly IJwtService _jwtService;
    private readonly IMemberRepository _memberRepository;

    public LoginWithFacebookCommandHandler(IJwtService jwtService, IMemberRepository memberRepository)
    {
        _jwtService = jwtService;
        _memberRepository = memberRepository;
    }

    public async Task<Result<TokenResponse>> Handle(LoginWithFacebookCommand request,
        CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(
                x => x.IsActive && x.MemberPlatformIdentityId == request.IdentityId &&
                     x.RegisterType == RegisterType.FACEBOOK,
                cancellationToken);
        if (member is null)
            return Result.Failure<TokenResponse>(MemberErrors.NotFound);
        var keycloakAccessToken =
            await _jwtService.ExchangeTokenAsync(request.AccessToken, "facebook", cancellationToken);
        return keycloakAccessToken.IsFailure
            ? Result.Failure<TokenResponse>(MemberErrors.InvalidCredentials)
            : Result.Success(keycloakAccessToken.Value);
    }
}