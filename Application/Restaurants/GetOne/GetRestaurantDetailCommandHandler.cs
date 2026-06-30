using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Restaurants.GetAll;
using Domain.Abstractions;
using Domain.Restaurants;
using Microsoft.EntityFrameworkCore;

namespace Application.Restaurants.GetOne;

public class GetRestaurantDetailCommandHandler : ICommandHandler<GetRestaurantDetailCommand, RestaurantResponse>
{
    private readonly IAwsS3Service _awsS3Service;
    private readonly IRestaurantRepository _restaurantRepository;

    public GetRestaurantDetailCommandHandler(IRestaurantRepository restaurantRepository, IAwsS3Service awsS3Service)
    {
        _restaurantRepository = restaurantRepository;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<RestaurantResponse>> Handle(GetRestaurantDetailCommand request,
        CancellationToken cancellationToken)
    {
        var result =
            await _restaurantRepository.GetEntitiesAsQueryable()
                .Include(x => x.RestaurantArea)
                .FirstOrDefaultAsync(x => x.Id.Equals(new RestaurantId(request.RestaurantId)), cancellationToken);
        if (result is null)
            return Result.Failure<RestaurantResponse>(RestaurantErrors.NotFound);
        var resultDto = new RestaurantResponse
        {
            Address = new AddressResponse
            {
                City = result.Address.City,
                Country = result.Address.Country,
                State = result.Address.State,
                Street = result.Address.Street,
                ZipCode = result.Address.ZipCode
            },
            OpeningAt = result.OpeningAt.ToShortTimeString(),
            ClosingAt = result.ClosingAt.ToShortTimeString(),
            IsAvailable = result.IsAvailable,
            AreaName = result.RestaurantArea?.AreaName.Value ?? "",
            RestaurantName = result.RestaurantName.Value,
            CreateDate = result.CreateDate,
            Id = result.Id.Value,
            MapLink = result.MapLink,
            AreaId = result.RestaurantAreaId?.Value ?? Guid.Empty,
            ImageUrl = result.ImageKey is null ? "" : _awsS3Service.GetUrlPresign(result.ImageKey.Value)
        };
        return Result.Success(resultDto);
    }
}