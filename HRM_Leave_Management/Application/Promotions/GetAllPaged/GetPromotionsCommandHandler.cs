using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Promotions.GetOne;
using Application.Restaurants.GetAll;
using Domain.Abstractions;
using Domain.Promotions;
using Microsoft.EntityFrameworkCore;

namespace Application.Promotions.GetAllPaged;

internal sealed class
    GetPromotionsCommandHandler : ICommandHandler<GetAllPagedPromotionsCommand, PagedList<PromotionResponse>>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IAwsS3Service _awsS3Service;

    public GetPromotionsCommandHandler(IPromotionRepository promotionRepository, IAwsS3Service awsS3Service)
    {
        _promotionRepository = promotionRepository;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<PagedList<PromotionResponse>>> Handle(GetAllPagedPromotionsCommand request,
        CancellationToken cancellationToken)
    {
        var query = _promotionRepository.GetEntitiesAsQueryable()
            .Include(p => p.PromotionToRestaurants)
            .ThenInclude(proToRes => proToRes.Restaurant)
            .OrderByDescending(x => x.CreatedDate)
            .AsQueryable();
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.AsEnumerable()
                .Where(x => x.Title.Value.ToLower().Contains(request.SearchTerm.ToLower()) ||
                            x.CreatedDate.ToString("dd/MM/yyyy").Contains(request.SearchTerm))
                .AsQueryable();
        }

        var result = await _promotionRepository.GetAllPaged(request, query);
        var promotionsResponse = result.Data.Select(p =>
        {
            var promotionRes = new PromotionResponse()
            {
                Id = p.Id.Value,
                PromotionName = p.PromotionName.Value,
                Content = p.Content.Value,
                Title = p.Title.Value,
                StartedAt = p.StartedAt,
                EndedAt = p.EndedAt,
                CreatedAt = p.CreatedDate,
                ImageUrl = _awsS3Service.GetUrlPresign(p.ImageUrl.Value),
            };
            return promotionRes;
        }).ToList();

        var resultList =
            new PagedList<PromotionResponse>(promotionsResponse, result.TotalCount, result.CurrentPage,
                result.PageSize);
        return Result.Success(resultList);
    }
}