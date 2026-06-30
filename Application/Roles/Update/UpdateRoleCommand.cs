using Application.Abstractions.Messaging;
using Domain.Roles;

namespace Application.Roles.Update;

public record UpdateRoleCommand(
    RoleId RoleId,
    string? ResourceName,
    string? DisplayName,
    List<Guid>? UserIds ,
    List<Guid>? PermissionIds 
    ) : ICommand<Role>;

