using System.Net.Http.Json;
using Application.Abstractions.Authentication;
using Application.Abstractions.GoogleAuth2;
using Domain.Abstractions;
using Infrastructure.Authentication.Models;
using Microsoft.Extensions.Options;

namespace Infrastructure.Authentication;

public class GoogleAuth2 : IGoogleAuth2
{
    private readonly HttpClient _httpClient;
    private readonly GoogleAuth2Options _auth2Options;
    private readonly IJwtService _jwtService;

    public GoogleAuth2(HttpClient httpClient, IOptions<GoogleAuth2Options> auth2Options, IJwtService jwtService)
    {
        _httpClient = httpClient;
        _jwtService = jwtService;
        _auth2Options = auth2Options.Value;
    }

    public async Task<Result<TokenResponse>> GetToken(string authCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var authRequestParameters = new KeyValuePair<string, string>[]
            {
                new("code", authCode),
                new("client_id", _auth2Options.ClientId),
                new("client_secret", _auth2Options.ClientSecret),
                new("redirect_uri", _auth2Options.RedirectUrl),
                new("grant_type", "authorization_code"),
            };

            var request = new FormUrlEncodedContent(authRequestParameters);

            var response = await _httpClient.PostAsync(_auth2Options.TokenUrl, request, cancellationToken);

            response.EnsureSuccessStatusCode();

            var authorizationToken = await response.Content.ReadFromJsonAsync<AuthorizationToken>();

            if (authorizationToken is null)
            {
                return Result.Failure<TokenResponse>(Error.None);
            }

            var keyCloakToken = await _jwtService.ExchangeTokenAsync(authorizationToken.AccessToken, "google");


            return Result.Success(keyCloakToken.Value);
        }
        catch (Exception e)
        {
            return Result.Failure<TokenResponse>(new Error("Google.Auth.Fail", "Fail to authentication"));
        }
    }
}