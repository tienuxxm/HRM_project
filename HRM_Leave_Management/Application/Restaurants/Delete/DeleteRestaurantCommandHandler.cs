using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Restaurants;

namespace Application.Restaurants.Delete;

internal sealed class DeleteRestaurantCommandHandler : ICommandHandler<DeleteRestaurantCommand>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRestaurantCommandHandler(IRestaurantRepository restaurantRepository, IUnitOfWork unitOfWork)
    {
        _restaurantRepository = restaurantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteRestaurantCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(request.RestaurantId);
        if (restaurant is null)
            return Result.Failure(RestaurantErrors.NotFound);
        restaurant.Delete();
        _restaurantRepository.Update(restaurant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}