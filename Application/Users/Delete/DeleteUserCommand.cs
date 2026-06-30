using Application.Abstractions.Messaging;
using Domain.Users;
using Domain.Vouchers;

namespace Application.Users.Delete;

public record DeleteUserCommand(UserId UserId) : ICommand;
