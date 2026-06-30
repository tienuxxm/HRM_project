using Application.Abstractions.Messaging;

namespace Application.Promotions.Create;

public sealed record CreatePromotionCommand(
    string Name,
    string Content,
    string Title,
    DateTime StartedAt,
    DateTime EndedAt,
    string? ImageUrl
    ) : ICommand<Guid>;