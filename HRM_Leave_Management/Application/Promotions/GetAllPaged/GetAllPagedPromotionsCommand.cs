using Application.Abstractions.Messaging;
using Application.Promotions.GetOne;
using Domain.Abstractions;
using Domain.Products;
using Domain.Promotions;

namespace Application.Promotions.GetAllPaged;

public sealed record GetAllPagedPromotionsCommand : PagedQuery<Promotion, PromotionId>, ICommand<PagedList<PromotionResponse>>;