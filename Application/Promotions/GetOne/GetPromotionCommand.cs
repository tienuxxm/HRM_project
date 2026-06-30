using Application.Abstractions.Messaging;
using Domain.Promotions;

namespace Application.Promotions.GetOne;

public sealed record GetPromotionCommand(PromotionId PromotionId) : ICommand<PromotionResponse>;