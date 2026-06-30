using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Promotions;
using Domain.Shared;

namespace Application.Promotions.Update;

internal sealed class UpdatePromotionCommandHandler : ICommandHandler<UpdatePromotionCommand>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePromotionCommandHandler(
        IUnitOfWork unitOfWork, IPromotionRepository promotionRepository)
    {
        _unitOfWork = unitOfWork;
        _promotionRepository = promotionRepository;
    }

    public async Task<Result> Handle(UpdatePromotionCommand request, CancellationToken cancellationToken)
    {
        var promotion = await _promotionRepository.GetByIdAsync(request.PromotionId);
        if (promotion is null)
        {
            return Result.Failure<Guid>(PromotionErrors.NotFound);
        }

        promotion.Update(
            new PromotionName(request.Name),
            new PromotionTitle(request.Title),
            new PromotionContent(request.Content),
            request.StartedAt,
            request.EndedAt,
            string.IsNullOrEmpty(request.ImageUrl) ? null : new ImageUrl(request.ImageUrl)
        );

        _promotionRepository.Update(promotion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}