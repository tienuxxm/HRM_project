using Application.Abstractions.Messaging;
using Application.Promotions.GetOne;
using Application.Restaurants.GetAll;
using Domain.Abstractions;
using Domain.Promotions;
using Domain.PromotionToRestaurants;
using Domain.Restaurants;
using Microsoft.EntityFrameworkCore;

namespace Application.Promotions.GetAll;

internal sealed class GetAllPromotionCommandHandler : ICommandHandler<GetAllPromotionCommand, List<PromotionResponse>>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IPromotionToRestaurantRepository _promotionToRestaurantRepository;
    private readonly IRestaurantRepository _restaurantRepository;

    public GetAllPromotionCommandHandler(IPromotionRepository promotionRepository,
        IPromotionToRestaurantRepository promotionToRestaurantRepository, IRestaurantRepository restaurantRepository)
    {
        _promotionRepository = promotionRepository;
        _promotionToRestaurantRepository = promotionToRestaurantRepository;
        _restaurantRepository = restaurantRepository;
    }

    public async Task<Result<List<PromotionResponse>>> Handle(GetAllPromotionCommand request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetAll(cancellationToken);
        var promotions = await _promotionRepository.GetEntitiesAsQueryable()
            .Include(x => x.PromotionToRestaurants)
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new PromotionResponse()
            {
                Content = x.Content.Value,
                Id = x.Id.Value,
                Title = x.Title.Value,
                PromotionName = x.PromotionName.Value,
                CreatedAt = x.CreatedDate.Date,
                EndedAt = x.EndedAt.Date,
                StartedAt = x.StartedAt.Date,
                ImageUrl = x.ImageUrl!.Value,
                Restaurants = restaurant!.Where(r =>
                        x.PromotionToRestaurants.Select(p => p.RestaurantId).ToList().Contains(r.Id))
                    .Select(r => new RestaurantResponse()
                    {
                        RestaurantName = r.RestaurantName.Value,
                    }).ToList()
            }).ToListAsync(cancellationToken);


        /*var result = await _promotionRepository.GetAll(cancellationToken);
        if (result is null)
            return Result.Success(new List<PromotionResponse>());
        var promotionsResponse = result.Select(p =>
        {
            var promotionRes = new PromotionResponse()
            {
                Id = p.Id.Value,
                PromotionName = p.PromotionName.Value,
                Content = p.Content.Value,
                Title = p.Title.Value,
                StartedAt = p.StartedAt,
                EndedAt = p.EndedAt,
                CreatedAt = p.CreatedDate,
                ImageUrl = p.ImageUrl.Value,
                restaurants = new List<RestaurantResponse>(),
            };
            if (p.PromotionToRestaurants != null)
            {
                promotionRes.restaurants =  p.PromotionToRestaurants.Select(val => new RestaurantResponse()
                {
                    Id = val.Restaurant.Id.Value,
                    RestaurantName = val.Restaurant.RestaurantName.Value,
                    Description = val.Restaurant.Description.Value,
                    Address = new AddressResponse()
                    {
                        Country = val.Restaurant.Address.Country,
                        City = val.Restaurant.Address.City,
                        State = val.Restaurant.Address.State,
                        ZipCode = val.Restaurant.Address.ZipCode,
                    },
                }).ToList();

            }
            return promotionRes;
        }).ToList();*/
        return Result.Success(promotions);
    }
}