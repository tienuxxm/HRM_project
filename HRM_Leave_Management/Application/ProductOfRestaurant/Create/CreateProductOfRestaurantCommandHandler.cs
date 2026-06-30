using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.ProductOfRestaurants;
using Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Application.ProductOfRestaurant.Create;

public class CreateProductOfRestaurantCommandHandler : ICommandHandler<CreateProductOfRestaurantCommand>
{
    private readonly IProductOfRestaurantRepository _productOfRestaurantRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductOfRestaurantCommandHandler(IProductOfRestaurantRepository productOfRestaurantRepository,
        IUnitOfWork unitOfWork, IProductRepository productRepository)
    {
        _productOfRestaurantRepository = productOfRestaurantRepository;
        _unitOfWork = unitOfWork;
        _productRepository = productRepository;
    }

    public async Task<Result> Handle(CreateProductOfRestaurantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var productList = await _productRepository.GetEntitiesAsQueryable()
                .Where(p => request.ProductIds.Any(x => x.Equals(p.Id))).ToListAsync(cancellationToken);

            var preparedSaveData = request.ProductIds.Select(pid =>
                Domain.ProductOfRestaurants.ProductOfRestaurant.Create(pid, request.RestaurantId,
                    productList.First(p => p.Id.Equals(pid)).AllowDelivery)).ToList();
            _productOfRestaurantRepository.AddRange(preparedSaveData);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(new Error("CreateProductRestaurant.FAIL", e.Message));
        }
    }
}