using System.Net;
using System.Net.Http.Json;
using Application.Abstractions.Authentication;
using Domain.Abstractions;
using Infrastructure.Authentication.Models;
using Microsoft.Extensions.Options;

namespace Infrastructure.Authentication;

internal sealed class JwtService : IJwtService
{
    private static readonly Error AuthenticationFailed = new(
        "Keycloak.AuthenticationFailed",
        "Failed to acquire access token do to authentication failure");

    private readonly HttpClient _httpClient;
    private readonly KeycloakOptions _keycloakOptions;

    public JwtService(HttpClient httpClient, IOptions<KeycloakOptions> keycloakOptions)
    {
        _httpClient = httpClient;
        _keycloakOptions = keycloakOptions.Value;
    }

    public async Task<Result<string>> GetAccessTokenAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var authRequestParameters = new KeyValuePair<string, string>[]
            {
                new("client_id", _keycloakOptions.AuthClientId),
                new("client_secret", _keycloakOptions.AuthClientSecret),
                new("scope", "openid email"),
                new("grant_type", "password"),
                new("username", email),
                new("password", password)
            };

            var authorizationRequestContent = new FormUrlEncodedContent(authRequestParameters);

            var response = await _httpClient.PostAsync("", authorizationRequestContent, cancellationToken);

            response.EnsureSuccessStatusCode();

            var authorizationToken = await response.Content.ReadFromJsonAsync<TokenResponse>();

            if (authorizationToken is null)
            {
                return Result.Failure<string>(AuthenticationFailed);
            }

            return Result.Success(authorizationToken.AccessToken);
        }
        catch (HttpRequestException e)
        {
            return Result.Failure<string>(AuthenticationFailed);
        }
    }

    public async Task<Result<TokenResponse>> GetAccessAndRefreshTokenAsync(string email, string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var authRequestParameters = new KeyValuePair<string, string>[]
            {
                new("client_id", _keycloakOptions.AuthClientId),
                new("client_secret", _keycloakOptions.AuthClientSecret),
                new("scope", "openid email"),
                new("grant_type", "password"),
                new("username", email),
                new("password", password)
            };
            var response = await SendRequest(authRequestParameters, cancellationToken);
            if (response is null)
                return Result.Failure<TokenResponse>(AuthenticationFailed);

            var authorizationToken = await response.Content.ReadFromJsonAsync<TokenResponse>();

            return authorizationToken is null
                ? Result.Failure<TokenResponse>(AuthenticationFailed)
                : Result.Success(authorizationToken);
        }
        catch (Exception exception)
        {
            return Result.Failure<TokenResponse>(AuthenticationFailed);
        }
    }

    public async Task<Result<TokenResponse>> ExchangeTokenAsync(string accessToken, string issuer,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var authRequestParameters = new KeyValuePair<string, string>[]
            {
                new("client_id", _keycloakOptions.AuthClientId),
                new("client_secret", _keycloakOptions.AuthClientSecret),
                new("grant_type", "urn:ietf:params:oauth:grant-type:token-exchange"),
                new("subject_token", accessToken),
                new("subject_issuer", issuer)
            };

            var response = await SendRequest(authRequestParameters, cancellationToken);
            if (response is null)
                return Result.Failure<TokenResponse>(AuthenticationFailed);
            if (response.StatusCode == HttpStatusCode.Conflict)
                return Result.Failure<TokenResponse>(AuthenticationErrors.EmailExisted);

            var authorizationToken = await response.Content.ReadFromJsonAsync<TokenResponse>();

            return authorizationToken is null
                ? Result.Failure<TokenResponse>(AuthenticationFailed)
                : Result.Success(authorizationToken);
        }
        catch (HttpRequestException exception)
        {
            return Result.Failure<TokenResponse>(AuthenticationFailed);
        }
    }

    public async Task<Result<TokenResponse>> RefreshToken(string refreshToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var authRequestParameters = new KeyValuePair<string, string>[]
            {
                new("client_id", _keycloakOptions.AuthClientId),
                new("client_secret", _keycloakOptions.AuthClientSecret),
                new("grant_type", "refresh_token"),
                new("refresh_token", refreshToken)
            };

            var response = await SendRequest(authRequestParameters, cancellationToken);
            if (response is null)
                return Result.Failure<TokenResponse>(AuthenticationFailed);

            var authorizationToken = await response.Content.ReadFromJsonAsync<TokenResponse>();

            if (authorizationToken is null)
            {
                return Result.Failure<TokenResponse>(AuthenticationFailed);
            }

            return Result.Success(authorizationToken);
        }
        catch (HttpRequestException)
        {
            return Result.Failure<TokenResponse>(AuthenticationFailed);
        }
    }

    private async Task<HttpResponseMessage?> SendRequest(KeyValuePair<string, string>[] authRequestParameters,
        CancellationToken cancellationToken)
    {
        var authorizationRequestContent = new FormUrlEncodedContent(authRequestParameters);

        var response = await _httpClient.PostAsync("", authorizationRequestContent, cancellationToken);

        response.EnsureSuccessStatusCode();
        return response;
    }
}