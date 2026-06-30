using Application.Abstractions.Messaging;
using Application.Promotions.GetOne;
using Domain.Promotions;

namespace Application.Promotions.GetAll;

public record GetAllPromotionCommand() : ICommand<List<PromotionResponse>>;
