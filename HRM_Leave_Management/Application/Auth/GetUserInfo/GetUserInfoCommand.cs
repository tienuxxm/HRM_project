using Application.Abstractions.Messaging;

namespace Application.Auth.GetUserInfo;

public record GetUserInfoCommand() : ICommand<UserInfoResponse>;