using Application.Abstractions.Messaging;
using Domain.Abstractions;

namespace Application.Auth;

public record RefreshTokenCommand(string RefreshToken) : ICommand<TokenResponse>;