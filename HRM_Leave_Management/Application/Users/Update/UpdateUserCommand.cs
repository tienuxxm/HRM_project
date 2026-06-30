using Application.Abstractions.Messaging;
using Domain.Users;
using Domain.Vouchers;

namespace Application.Users.Update;

public sealed record UpdateUserCommand(
    UserId UserId,
    string? Name,
    string? Email,
    string? PhoneNumber,
    List<Guid>? RoleIds
        ) : ICommand<User>;

