using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Users;

namespace Application.Users.GetAllPaged;

public record GetAllUserPagedCommand() : PagedQuery<User, UserId>, ICommand<GetAllUserPagedResponse>;