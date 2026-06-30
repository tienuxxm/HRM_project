using Application.Abstractions.Messaging;
using Domain.Roles;
using Domain.Users;

namespace Application.Roles.GetOne;

public record GetOneRoleCommand(RoleId RoleId) :  ICommand<RoleResponse>;
