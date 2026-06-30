using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Members;

namespace Application.Auth;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, TokenResponse>
{
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(IJwtService jwtService)
    {
        _jwtService = jwtService;
    }

    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var result = await _jwtService.RefreshToken(request.RefreshToken, cancellationToken);
        return result.IsFailure
            ? Result.Failure<TokenResponse>(new Error("RefreshToken.Fail", "Session expired"))
            : Result.Success(result.Value);
    }
}