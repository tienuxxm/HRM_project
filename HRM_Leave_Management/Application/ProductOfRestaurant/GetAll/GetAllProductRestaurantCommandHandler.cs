using Application.Abstractions.Messaging;
using Application.ProductOfRestaurant.Response;
using Domain.Abstractions;
using Domain.ProductOfRestaurants;
using Domain.Restaurants;
using Microsoft.EntityFrameworkCore;

namespace Application.ProductOfRestaurant.GetAll;

public class
    GetAllProductRestaurantCommandHandler : ICommandHandler<GetAllProductRestaurantCommand,
        List<ProductOfRestaurantResponse>>
{
    private readonly IProductOfRestaurantRepository _productOfRestaurantRepository;

    public GetAllProductRestaurantCommandHandler(IProductOfRestaurantRepository productOfRestaurantRepository)
    {
        _productOfRestaurantRepository = productOfRestaurantRepository;
    }

    public async Task<Result<List<ProductOfRestaurantResponse>>> Handle(GetAllProductRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = _productOfRestaurantRepository.GetEntitiesAsQueryable()
                .Include(x => x.Product)
                .Include(x => x.Restaurant);

            var data = (await query
                    .ToListAsync(cancellationToken))
                .GroupBy(x => new { x.RestaurantId.Value })
                .Select(x => new ProductOfRestaurantResponse()
                {
                    RestaurantName = query.First(r => r.RestaurantId.Equals(new RestaurantId(x.Key.Value))).Restaurant
                        .RestaurantName.Value,
                    TotalProduct = query.Count(r => r.RestaurantId.Equals(new RestaurantId(x.Key.Value)))
                }).ToList();

            return Result.Success(data);
        }
        catch (Exception e)
        {
            return Result.Failure<List<ProductOfRestaurantResponse>>(new Error("GetProductRestaurant.Fail", e.Message));
        }
    }
}