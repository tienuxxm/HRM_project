using Domain.Abstractions;
using Domain.Members;
using Domain.Users;

namespace Application.Abstractions.Authentication;

public interface IAuthenticationService
{
    Task<Result<string>> RegisterAsync(
        Member user,
        string password,
        CancellationToken cancellationToken = default);

    Task<Result<string>> RegisterAsync(
        User user,
        string password,
        CancellationToken cancellationToken = default);

    Task<Result> ResetPassword(string newPassword, string userId, CancellationToken cancellationToken = default);

    Task<Result> ChangeEmail(string newEmail, string userId, CancellationToken cancellationToken = default);

    Task<Result> DeleteUser(string userId, CancellationToken cancellationToken);
}