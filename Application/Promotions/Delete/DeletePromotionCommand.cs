using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Products;
using Domain.Promotions;

namespace Application.Promotions.Delete;

public record DeletePromotionCommand(PromotionId PromotionId) : ICommand<BooleanResponse>;