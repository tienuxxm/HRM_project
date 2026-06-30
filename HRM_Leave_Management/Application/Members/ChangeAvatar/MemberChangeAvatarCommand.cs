using Application.Abstractions.Messaging;

namespace Application.Members.ChangeAvatar;

public record MemberChangeAvatarCommand(string Image) : ICommand;