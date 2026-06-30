using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Restaurants;

namespace Application.Restaurants.ToggleAvailable;

public class ToggleAvailableRestaurantCommandHandler : ICommandHandler<ToggleAvailableRestaurantCommand>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ToggleAvailableRestaurantCommandHandler(IRestaurantRepository restaurantRepository, IUnitOfWork unitOfWork)
    {
        _restaurantRepository = restaurantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ToggleAvailableRestaurantCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(request.RestaurantId, cancellationToken);
        if (restaurant is null)
            return Result.Failure(RestaurantErrors.NotFound);
        if (request.Toggle)
        {
            restaurant.SetAvailable();
        }
        else
        {
            restaurant.SetUnavailable();
        }

        _restaurantRepository.Update(restaurant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}