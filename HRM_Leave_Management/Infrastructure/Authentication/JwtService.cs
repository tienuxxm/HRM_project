using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Application.Abstractions.Authentication;
using Domain.Abstractions;
using Infrastructure.Authentication.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Authentication;

internal sealed class JwtService : IJwtService
{
    private static readonly Error AuthenticationFailed = new(
        "Keycloak.AuthenticationFailed",
        "Failed to acquire access token do to authentication failure");

    private readonly HttpClient _httpClient;
    private readonly KeycloakOptions _keycloakOptions;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly AuthenticationOptions _authenticationOptions;

    public JwtService(
        HttpClient httpClient, 
        IOptions<KeycloakOptions> keycloakOptions,
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        IOptions<AuthenticationOptions> authenticationOptions)
    {
        _httpClient = httpClient;
        _keycloakOptions = keycloakOptions.Value;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _authenticationOptions = authenticationOptions.Value;
    }

    public async Task<Result<string>> GetAccessTokenAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var useMockAuth = _configuration.GetValue<bool>("UseMockAuth");
        if (useMockAuth)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var user = dbContext.Set<Domain.Users.User>()
                .AsEnumerable()
                .FirstOrDefault(u => u.Email.Value.Equals(email, StringComparison.OrdinalIgnoreCase));
                
            if (user is null)
            {
                return Result.Failure<string>(AuthenticationFailed);
            }

            var identityId = user.IdentityId.Value;

            var claims = new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, identityId),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, email),
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, identityId),
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Email, email),
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretMockKeyForLocalDevelopmentOnlyDontUseInProduction123!"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _authenticationOptions.Issuer,
                audience: _authenticationOptions.Audience,
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Result.Success(tokenString);
        }

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
        catch (HttpRequestException)
        {
            return Result.Failure<string>(AuthenticationFailed);
        }
    }

    public async Task<Result<TokenResponse>> GetAccessAndRefreshTokenAsync(string email, string password,
        CancellationToken cancellationToken = default)
    {
        var useMockAuth = _configuration.GetValue<bool>("UseMockAuth");
        if (useMockAuth)
        {
            var tokenResult = await GetAccessTokenAsync(email, password, cancellationToken);
            if (tokenResult.IsFailure)
                return Result.Failure<TokenResponse>(tokenResult.Error);

            return Result.Success(new TokenResponse
            {
                AccessToken = tokenResult.Value,
                RefreshToken = "mock-refresh-token",
                ExpiredIn = 3600
            });
        }

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
        catch (Exception)
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
        catch (HttpRequestException)
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