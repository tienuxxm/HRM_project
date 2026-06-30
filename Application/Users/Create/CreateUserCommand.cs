using Application.Abstractions.Messaging;
using Domain.Users;

namespace Application.Users.Create;

public sealed record CreateUserCommand(
    string Name,
    string Username,
    string Password,
    List<Guid>? RoleIds,
    string? Email,
    string? PhoneNumber
    ) : ICommand<Guid>;

