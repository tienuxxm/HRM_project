using Application.Abstractions.Messaging;
using Domain.Users;

namespace Application.Users.GetOne;

public record GetOneUserQuery(UserId UserId) :  IQuery<UserResponse>;
