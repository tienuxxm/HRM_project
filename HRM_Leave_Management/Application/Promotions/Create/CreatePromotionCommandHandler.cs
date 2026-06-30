using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Categories;
using Domain.Promotions;
using Domain.PromotionToRestaurants;
using Domain.Restaurants;
using Domain.Shared;

namespace Application.Promotions.Create;

internal class CreatePromotionCommandHandler : ICommandHandler<CreatePromotionCommand, Guid>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPromotionToRestaurantRepository _promotionToRestaurantRepository;
    private readonly IRestaurantRepository _restaurantRepository;

    public CreatePromotionCommandHandler(
        IPromotionRepository productRepository,
        IDateTimeProvider dateTimeProvider,
        IPromotionToRestaurantRepository promotionToRestaurantRepository,
        IRestaurantRepository restaurantRepository,
        IUnitOfWork unitOfWork)
    {
        _restaurantRepository = restaurantRepository;
        _promotionRepository = productRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
        _promotionToRestaurantRepository = promotionToRestaurantRepository;
    }

    public async Task<Result<Guid>> Handle(CreatePromotionCommand request, CancellationToken cancellationToken)
    {
        var promotion = Promotion.Create(
            new PromotionName(request.Name),
            new PromotionTitle(request.Title),
            new PromotionContent(request.Content),
            request.StartedAt,
            request.EndedAt,
            _dateTimeProvider.UtcNow,
            string.IsNullOrEmpty(request.ImageUrl) ? null : new ImageUrl(request.ImageUrl));
        _promotionRepository.Add(promotion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return promotion.Id.Value;
    }
}