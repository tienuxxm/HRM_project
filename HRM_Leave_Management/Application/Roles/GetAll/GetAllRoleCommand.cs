using Application.Abstractions.Messaging;
using Application.Roles.GetOne;

namespace Application.Roles.GetAll;

public record GetAllRoleCommand(int? Take, int? Skip, string? Search) :  ICommand<List<RoleResponse>?>;
