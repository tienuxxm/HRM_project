using Application.Abstractions.Messaging;

namespace Application.Members.Import;

public record ImportMembersCommand(MemoryStream Stream) : ICommand;