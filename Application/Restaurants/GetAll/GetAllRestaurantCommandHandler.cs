using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Restaurants;

namespace Application.Restaurants.GetAll;

internal sealed class
    GetAllRestaurantCommandHandler : ICommandHandler<GetAllRestaurantCommand, List<RestaurantResponse>>
{
    private readonly IAwsS3Service _awsS3Service;
    private readonly IRestaurantRepository _restaurantRepository;

    public GetAllRestaurantCommandHandler(IRestaurantRepository restaurantRepository, IAwsS3Service awsS3Service)
    {
        _restaurantRepository = restaurantRepository;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<List<RestaurantResponse>>> Handle(GetAllRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _restaurantRepository.GetAllWithOutDeleted(cancellationToken);
        var resultDtos = result?.Select(r => new RestaurantResponse
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
            IsAvailable = r.IsAvailable,
            AreaName = r.RestaurantArea?.AreaName.Value ?? "",
            CreateDate = r.CreateDate,
            RestaurantName = r.RestaurantName.Value,
            MapLink = r.MapLink,
            Id = r.Id.Value,
            AreaId = r.RestaurantAreaId?.Value ?? Guid.Empty,
            ImageUrl = r.ImageKey is null ? "" : _awsS3Service.GetUrlPresign(r.ImageKey.Value)
        }).ToList();
        return Result.Success(resultDtos ?? new List<RestaurantResponse>());
    }
}