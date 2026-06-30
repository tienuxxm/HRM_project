using Domain.Abstractions;

namespace Application.Abstractions.GoogleAuth2;

public interface IGoogleAuth2
{
    Task<Result<TokenResponse>> GetToken(string authCode, CancellationToken cancellationToken = default);
}