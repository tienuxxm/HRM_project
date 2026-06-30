using Application.Abstractions.Messaging;
using Domain.Promotions;
using Domain.Restaurants;

namespace Application.Promotions.Update;

public record UpdatePromotionCommand(
    PromotionId PromotionId,
    string? Name,
    string? Content,
    string? Title,
    string? ImageUrl,
    DateTime? StartedAt,
    DateTime? EndedAt
    ) : ICommand;