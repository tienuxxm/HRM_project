using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.Products;
using Domain.Promotions;

namespace Application.Promotions.Delete;

internal sealed class DeletePromotionCommandHandler : ICommandHandler<DeletePromotionCommand, BooleanResponse>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePromotionCommandHandler(IPromotionRepository productRepository, IUnitOfWork unitOfWork)
    {
        _promotionRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(DeletePromotionCommand request, CancellationToken cancellationToken)
    {
        var promotion = await _promotionRepository.GetByIdAsync(request.PromotionId, cancellationToken);
        if (promotion is null)
        {
            return Result.Failure<BooleanResponse>(ProductErrors.NotFound);
        }

        _promotionRepository.Remove(promotion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new BooleanResponse { Result = true });
    }
}