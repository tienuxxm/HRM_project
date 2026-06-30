using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Restaurants;
using Domain.Vouchers;

namespace Application.Restaurants.Update;

internal sealed class UpdateRestaurantCommandHandler : ICommandHandler<UpdateRestaurantCommand, Guid>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRestaurantCommandHandler(IRestaurantRepository restaurantRepository, IUnitOfWork unitOfWork,
        IVoucherRepository voucherRepository)
    {
        _restaurantRepository = restaurantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(UpdateRestaurantCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(request.Id, cancellationToken);
        if (restaurant is null)
        {
            return Result.Failure<Guid>(RestaurantErrors.NotFound);
        }

        restaurant.Update(
            request.RestaurantName,
            request.Address,
            request.OpeningAt,
            request.ClosingAt,
            request.RestaurantAreaId,
            request.ImageUrl,
            request.mapLink
        );
        _restaurantRepository.Update(restaurant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(restaurant.Id.Value);
    }
}