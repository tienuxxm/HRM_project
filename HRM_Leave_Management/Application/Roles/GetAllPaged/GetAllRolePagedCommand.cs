using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Roles;
using Domain.Users;

namespace Application.Roles.GetAllPaged;

public record GetAllRolePagedCommand() : PagedQuery<Role, RoleId>, ICommand<GetAllRolePagedResponse>;