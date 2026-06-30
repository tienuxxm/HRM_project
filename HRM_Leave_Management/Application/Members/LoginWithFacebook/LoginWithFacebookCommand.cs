using Application.Abstractions.Messaging;
using Domain.Abstractions;

namespace Application.Members.LoginWithFacebook;

public record LoginWithFacebookCommand(string AccessToken, string IdentityId) : ICommand<TokenResponse>;