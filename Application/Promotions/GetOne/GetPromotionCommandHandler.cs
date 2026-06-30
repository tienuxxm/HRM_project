using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Restaurants.GetAll;
using Domain.Abstractions;
using Domain.Promotions;

namespace Application.Promotions.GetOne;

internal sealed class GetPromotionCommandHandler : ICommandHandler<GetPromotionCommand, PromotionResponse>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IAwsS3Service _awsS3Service;

    public GetPromotionCommandHandler(IPromotionRepository promotionRepository, IAwsS3Service awsS3Service)
    {
        _promotionRepository = promotionRepository;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<PromotionResponse>> Handle(GetPromotionCommand request,
        CancellationToken cancellationToken)
    {
        var promotion = await _promotionRepository.GetByIdAsync(request.PromotionId);
        if (promotion is null)
        {
            return Result.Failure<PromotionResponse>(PromotionErrors.NotFound);
        }

        var promotionResponse = new PromotionResponse()
        {
            Id = promotion.Id.Value,
            PromotionName = promotion.PromotionName.Value,
            Content = promotion.Content.Value,
            Title = promotion.Title.Value,
            StartedAt = promotion.StartedAt,
            EndedAt = promotion.EndedAt,
            CreatedAt = promotion.CreatedDate,
            ImageUrl = _awsS3Service.GetUrlPresign(promotion.ImageUrl.Value),
        };
       
        return Result.Success(promotionResponse);
    }
}