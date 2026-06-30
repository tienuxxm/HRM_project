using Application.Abstractions.Messaging;

namespace Application.Members.DeleteMyAccount;

public record DeleteMyAccountCommand : ICommand<bool>;