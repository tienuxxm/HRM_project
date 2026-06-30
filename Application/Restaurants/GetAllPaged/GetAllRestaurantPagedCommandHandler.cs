using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Restaurants.GetAll;
using Domain.Abstractions;
using Domain.Restaurants;
using Microsoft.EntityFrameworkCore;

namespace Application.Restaurants.GetAllPaged;

public class
    GetAllRestaurantPagedCommandHandler : ICommandHandler<GetAllRestaurantPagedCommand, PagedList<RestaurantResponse>>
{
    private readonly IAwsS3Service _awsS3Service;
    private readonly IRestaurantRepository _restaurantRepository;

    public GetAllRestaurantPagedCommandHandler(IRestaurantRepository restaurantRepository, IAwsS3Service awsS3Service)
    {
        _restaurantRepository = restaurantRepository;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<PagedList<RestaurantResponse>>> Handle(GetAllRestaurantPagedCommand request,
        CancellationToken cancellationToken)
    {
        var query = _restaurantRepository.GetEntitiesAsQueryable()
            .Include(x => x.RestaurantArea)
            .Where(x => !x.IsDeleted);
        var result = await _restaurantRepository.GetAllPaged(request, query);
        var restaurantResponse = result.Data.Select(r => new RestaurantResponse
        {
            Address = new AddressResponse
            {
                City = r.Address.City,
                Country = r.Address.Country,
                State = r.Address.State,
                Street = r.Address.Street,
                ZipCode = r.Address.ZipCode
            },
            IsAvailable = r.IsAvailable,
            OpeningAt = r.OpeningAt.ToShortTimeString(),
            ClosingAt = r.ClosingAt.ToShortTimeString(),
            AreaName = r.RestaurantArea?.AreaName.Value ?? "",
            RestaurantName = r.RestaurantName.Value,
            Id = r.Id.Value,
            MapLink = r.MapLink,
            AreaId = r.RestaurantAreaId?.Value ?? Guid.Empty,
            ImageUrl = r.ImageKey is null ? "" : _awsS3Service.GetUrlPresign(r.ImageKey.Value)
        }).ToList();
        return Result.Success(new PagedList<RestaurantResponse>(restaurantResponse, result.TotalCount,
            result.CurrentPage,
            request.PageSize));
    }
}