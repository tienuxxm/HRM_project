using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Restaurants;

namespace Application.Restaurants.Create;

public class CreateRestaurantCommandHandler : ICommandHandler<CreateRestaurantCommand, Guid>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateRestaurantCommandHandler(IRestaurantRepository restaurantRepository, IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _restaurantRepository = restaurantRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Guid>> Handle(CreateRestaurantCommand request, CancellationToken cancellationToken)
    {
        var restaurant = Restaurant.Create(
            request.RestaurantAreaId,
            request.RestaurantName,
            request.Address, _dateTimeProvider.UtcNow,
            request.OpeningAt,
            request.ClosingAt,
            request.ImageUrl,
            request.mapLink);
        _restaurantRepository.Add(restaurant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(restaurant.Id.Value);
    }
}