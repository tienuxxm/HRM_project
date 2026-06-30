using Application.Abstractions.Messaging;

namespace Application.Auth.ResetPassword;

public record ResetPasswordCommand(string PhoneNumber, string newPassword) : ICommand<bool>;