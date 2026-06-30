using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.RestaurantAreas;

namespace Application.RestaurantArea.Create;

public class CreateAreaCommandHandler : ICommandHandler<CreateAreaCommand, Guid>
{
    private readonly IRestaurantAreaRepository _restaurantAreaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateAreaCommandHandler(IRestaurantAreaRepository restaurantAreaRepository,
        IDateTimeProvider dateTimeProvider, IUnitOfWork unitOfWork)
    {
        _restaurantAreaRepository = restaurantAreaRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateAreaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var area = Domain.RestaurantAreas.RestaurantArea.Create(new AreaName(request.AreaName),
                _dateTimeProvider.UtcNow);
            _restaurantAreaRepository.Add(area);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(area.Id.Value);
        }
        catch (Exception)
        {
            return Result.Failure<Guid>(RestaurantAreaError.CreateFail);
        }
    }
}