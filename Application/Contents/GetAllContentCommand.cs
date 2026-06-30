using Application.Abstractions.Messaging;

namespace Application.Contents;

public record GetAllContentCommand() : ICommand<ContentResponse>;