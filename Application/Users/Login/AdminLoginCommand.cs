using Application.Abstractions.Messaging;

namespace Application.Users.Login;

public record AdminLoginCommand(string username, string password) : ICommand<AccessTokenResponse>;