using Application.Abstractions.Messaging;
using Domain.Abstractions;

namespace Application.Auth;

public record LoginMemberCommand(string Phone, string Password, string? DeviceToken = null) : ICommand<TokenResponse>;