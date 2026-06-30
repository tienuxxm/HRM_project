using Application.Abstractions.Messaging;

namespace Application.Members.IsFacebookMemberExisted;

public record IsFacebookMemberExistedCommand(string IdentityId) : ICommand<bool>;