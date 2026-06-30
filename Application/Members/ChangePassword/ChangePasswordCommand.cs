using Application.Abstractions.Messaging;

namespace Application.Members.ChangePassword;

public record ChangePasswordCommand(string NewPassword, string CurrentPassword) : ICommand;