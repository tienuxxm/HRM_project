using Application.Abstractions.Messaging;
using Domain.Partners;
using Domain.Roles;
using Domain.Vouchers;

namespace Application.Roles.Delete;

public record DeleteRoleCommand(RoleId RoleId) : ICommand;
