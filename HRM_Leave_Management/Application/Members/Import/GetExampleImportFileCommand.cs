using Application.Abstractions.Messaging;

namespace Application.Members.Import;

public record GetExampleImportFileCommand() : ICommand<string>;