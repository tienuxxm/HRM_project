using Domain.Abstractions;

namespace Application.Abstractions.Authentication;

public interface IJwtService
{
    Task<Result<string>> GetAccessTokenAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<Result<TokenResponse>> GetAccessAndRefreshTokenAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<Result<TokenResponse>> ExchangeTokenAsync(
        string accessToken, string issuer,
        CancellationToken cancellationToken = default);

    Task<Result<TokenResponse>> RefreshToken(string refreshToken, CancellationToken cancellationToken = default);
}