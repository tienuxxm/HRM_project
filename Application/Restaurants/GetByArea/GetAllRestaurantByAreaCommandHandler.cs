using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Restaurants.GetAll;
using Domain.Abstractions;
using Domain.Restaurants;
using Microsoft.EntityFrameworkCore;

namespace Application.Restaurants.GetByArea;

public class
    GetAllRestaurantByAreaCommandHandler : ICommandHandler<GetAllRestaurantByAreaCommand,
    List<RestaurantByAreaResponse>>
{
    private readonly IAwsS3Service _awsS3Service;
    private readonly IRestaurantRepository _restaurantRepository;

    public GetAllRestaurantByAreaCommandHandler(IRestaurantRepository restaurantRepository, IAwsS3Service awsS3Service)
    {
        _restaurantRepository = restaurantRepository;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<List<RestaurantByAreaResponse>>> Handle(GetAllRestaurantByAreaCommand request,
        CancellationToken cancellationToken)
    {
        var query = _restaurantRepository
            .GetEntitiesAsQueryable()
            .Include(r => r.RestaurantArea)
            .Where(r => !r.IsDeleted);

        var restaurantList = (await query
                .ToListAsync(cancellationToken))
            .GroupBy(x => new { x.RestaurantArea.AreaName })
            .Select(x => new RestaurantByAreaResponse
            {
                AreaName = x.Key.AreaName.Value,
                Restaurants = query
                    .Where(y => y.RestaurantArea.AreaName.Equals(x.Key.AreaName))
                    .Select(r => new RestaurantResponse
                    {
                        Address = new AddressResponse
                        {
                            City = r.Address.City,
                            Country = r.Address.Country,
                            State = r.Address.State,
                            Street = r.Address.Street,
                            ZipCode = r.Address.ZipCode
                        },
                        OpeningAt = r.OpeningAt.ToShortTimeString(),
                        ClosingAt = r.ClosingAt.ToShortTimeString(),
                        AreaName = r.RestaurantArea.AreaName.Value,
                        CreateDate = r.CreateDate,
                        IsAvailable = r.IsAvailable,
                        MapLink = r.MapLink,
                        RestaurantName = r.RestaurantName.Value,
                        Id = r.Id.Value,
                        AreaId = r.RestaurantAreaId.Value,
                        ImageUrl = r.ImageKey == null ? "" : _awsS3Service.GetUrlPresign(r.ImageKey.Value, 60)
                    }).ToList()
            }).ToList();
        return Result.Success(restaurantList);
    }
}