using Application.Abstractions.Messaging;
using Domain.Abstractions;

namespace Application.Members.RegisterWithFacebook;

public record RegisterWithFacebookCommand(string AccessToken, string IdentityId, string Firstname, string Lastname,
    string PhoneNumber, string Email, string Address, int? DistrictId) : ICommand<TokenResponse>;