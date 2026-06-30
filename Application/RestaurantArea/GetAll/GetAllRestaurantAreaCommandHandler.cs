using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.RestaurantAreas;
using Microsoft.EntityFrameworkCore;

namespace Application.RestaurantArea.GetAll;

public class
    GetAllRestaurantAreaCommandHandler : ICommandHandler<GetAllRestaurantAreaCommand, List<RestaurantAreaResponse>>
{
    private readonly IRestaurantAreaRepository _restaurantAreaRepository;

    public GetAllRestaurantAreaCommandHandler(IRestaurantAreaRepository restaurantAreaRepository)
    {
        _restaurantAreaRepository = restaurantAreaRepository;
    }

    public async Task<Result<List<RestaurantAreaResponse>>> Handle(GetAllRestaurantAreaCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _restaurantAreaRepository.GetEntitiesAsQueryable()
            .Where(x => x.IsActive).ToListAsync(cancellationToken);
        var resultResponse = result.Select(x => new RestaurantAreaResponse()
        {
            Id = x.Id.Value,
            AreaName = x.AreaName.Value
        }).ToList();
        return Result.Success(resultResponse);
    }
}