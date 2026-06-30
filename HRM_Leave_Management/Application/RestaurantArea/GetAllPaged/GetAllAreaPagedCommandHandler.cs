using Application.Abstractions.Messaging;
using Application.RestaurantArea.GetAll;
using Domain.Abstractions;
using Domain.RestaurantAreas;

namespace Application.RestaurantArea.GetAllPaged;

public class GetAllAreaPagedCommandHandler : ICommandHandler<GetAllAreaPagedCommand, PagedList<RestaurantAreaResponse>>
{
    private readonly IRestaurantAreaRepository _restaurantAreaRepository;

    public GetAllAreaPagedCommandHandler(IRestaurantAreaRepository restaurantAreaRepository)
    {
        _restaurantAreaRepository = restaurantAreaRepository;
    }

    public async Task<Result<PagedList<RestaurantAreaResponse>>> Handle(GetAllAreaPagedCommand request,
        CancellationToken cancellationToken)
    {
        var query = _restaurantAreaRepository.GetEntitiesAsQueryable()
            .Where(x => x.IsActive);
        var result = await _restaurantAreaRepository.GetAllPaged(request, query);
        var dataResponse = result.Data.Select(x => new RestaurantAreaResponse()
        {
            Id = x.Id.Value,
            AreaName = x.AreaName.Value
        }).ToList();

        return Result.Success(new PagedList<RestaurantAreaResponse>(dataResponse, result.TotalCount, result.CurrentPage,
            result.CurrentPage));
    }
}