using Application.Abstractions.Messaging;
using Application.Users.GetOne;
using Domain.Users;

namespace Application.Users.GetAll;

public record GetAllUserQuery(int Take, int Skip, string? Search) :  IQuery<List<UserResponse>?>;
