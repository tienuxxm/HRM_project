using Application.Abstractions.Messaging;
using Domain.Partners;
using Domain.Users;
using Domain.Vouchers;

namespace Application.Roles.Create;

public sealed record CreateRoleCommand(
    string ResourceName,
    string DisplayName,
    List<Guid>? UserIds,
    List<Guid>? PermissionIds )  : ICommand<Guid>;

