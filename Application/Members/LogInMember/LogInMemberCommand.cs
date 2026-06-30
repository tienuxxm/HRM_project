using Application.Abstractions.Messaging;

namespace Application.Members.LogInMember;

public sealed record LogInMemberCommand(string Phone, string Password, string? deviceToken = null)
    : ICommand<AccessTokenResponse>;