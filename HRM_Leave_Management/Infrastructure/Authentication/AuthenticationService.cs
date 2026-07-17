using System.Net;
using System.Net.Http.Json;
using Application.Abstractions.Authentication;
using Domain.Abstractions;
using Domain.Members;
using Domain.Users;
using Infrastructure.Authentication.Models;

namespace Infrastructure.Authentication;

internal sealed class AuthenticationService : IAuthenticationService
{
    private const string PasswordCredentialType = "password";

    private readonly HttpClient _httpClient;

    public AuthenticationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<string>> RegisterAsync(
        Member member,
        string password,
        CancellationToken cancellationToken = default)
    {
        var userRepresentationModel = MemberRepresentationModel.FromUser(member);

        userRepresentationModel.Credentials = new CredentialRepresentationModel[]
        {
            new()
            {
                Value = password,
                Temporary = false,
                Type = PasswordCredentialType
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            "users",
            userRepresentationModel,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return Result.Failure<string>(AuthenticationErrors.EmailExisted);
        }

        if (response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.OK)
        {
            return Result.Failure<string>(AuthenticationErrors.ServerError);
        }

        return Result.Success(ExtractIdentityIdFromLocationHeader(response));
    }
    
    public async Task<Result<string>> RegisterAsync(
            User member,
            string password,
            CancellationToken cancellationToken = default)
        {
            var userRepresentationModel = MemberRepresentationModel.FromUser(member);
    
            userRepresentationModel.Credentials = new CredentialRepresentationModel[]
            {
                new()
                {
                    Value = password,
                    Temporary = false,
                    Type = PasswordCredentialType
                }
            };
    
            var response = await _httpClient.PostAsJsonAsync(
                "users",
                userRepresentationModel,
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                var conflictError = await GetConflictErrorAsync(response, cancellationToken);
                return Result.Failure<string>(conflictError);
            }

            if (response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.OK)
            {
                return Result.Failure<string>(AuthenticationErrors.ServerError);
            }

            return Result.Success(ExtractIdentityIdFromLocationHeader(response));
        }

    public async Task<Result> ResetPassword(string newPassword, string userId, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PutAsJsonAsync(
            $"users/{userId}/reset-password",
            new
            {
                Type = "password",
                Temporary = false,
                Value = newPassword
            }, cancellationToken);
        return result.StatusCode == HttpStatusCode.NoContent
            ? Result.Success()
            : Result.Failure(AuthenticationErrors.ChangePasswordError);
    }

    public async Task<Result> ChangeEmail(string newEmail, string userId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync($"users/{userId}", new { Email = newEmail },
            cancellationToken);
        return response.StatusCode switch
        {
            HttpStatusCode.Conflict => Result.Failure<string>(AuthenticationErrors.EmailExisted),
            HttpStatusCode.Created or HttpStatusCode.OK or HttpStatusCode.NoContent => Result.Success(),
            _ => Result.Failure<string>(
                AuthenticationErrors.ServerError)
        };
    }

    public async Task<Result> DeleteUser(string userId, CancellationToken cancellationToken)
    {
        var result = await _httpClient.DeleteAsync(
            $"users/{userId}", cancellationToken);
        return (result.StatusCode == HttpStatusCode.NoContent || result.StatusCode == HttpStatusCode.NotFound)
            ? Result.Success()
            : Result.Failure(AuthenticationErrors.DeleteUserError);
    }

    private static async Task<Error> GetConflictErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!string.IsNullOrEmpty(content))
            {
                if (content.Contains("username", StringComparison.OrdinalIgnoreCase))
                {
                    return AuthenticationErrors.UsernameExisted;
                }
                if (content.Contains("email", StringComparison.OrdinalIgnoreCase))
                {
                    return AuthenticationErrors.EmailExisted;
                }
            }
        }
        catch
        {
            // Ignore exception and return neutral error
        }
        return AuthenticationErrors.UserAlreadyExists;
    }

    private static string ExtractIdentityIdFromLocationHeader(
        HttpResponseMessage httpResponseMessage)
    {
        const string usersSegmentName = "users/";

        var locationHeader = httpResponseMessage.Headers.Location?.PathAndQuery;

        if (locationHeader is null) throw new InvalidOperationException("Location header can't be null");

        var userSegmentValueIndex = locationHeader.IndexOf(
            usersSegmentName,
            StringComparison.InvariantCultureIgnoreCase);

        var userIdentityId = locationHeader.Substring(
            userSegmentValueIndex + usersSegmentName.Length);

        return userIdentityId;
    }
}