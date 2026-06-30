using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.RestaurantAreas;

namespace Application.RestaurantArea.Delete;

public class DeleteAreaCommandHandler : ICommandHandler<DeleteAreaCommand>
{
    private readonly IRestaurantAreaRepository _restaurantAreaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAreaCommandHandler(IRestaurantAreaRepository restaurantAreaRepository, IUnitOfWork unitOfWork)
    {
        _restaurantAreaRepository = restaurantAreaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteAreaCommand request, CancellationToken cancellationToken)
    {
        var area = await _restaurantAreaRepository.GetByIdAsync(new RestaurantAreaId(request.id), cancellationToken);
        if (area is null)
            return Result.Failure(RestaurantAreaError.NotFound);
        area.Deactivate();
        _restaurantAreaRepository.Update(area);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}