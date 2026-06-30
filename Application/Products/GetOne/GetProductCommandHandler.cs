using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Categories.GetOne;
using Application.Restaurants.GetAll;
using Domain.Abstractions;
using Domain.Products;

namespace Application.Products.GetOne;

internal sealed class GetProductCommandHandler : ICommandHandler<GetProductCommand, ProductResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IAwsS3Service _awsS3Service;

    public GetProductCommandHandler(IProductRepository productRepository, IAwsS3Service awsS3Service)
    {
        _productRepository = productRepository;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<ProductResponse>> Handle(GetProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id);
        if (product is null)
            return Result.Failure<ProductResponse>(ProductErrors.NotFound);
        return Result.Success(new ProductResponse()
        {
            Id = product.Id.Value,
            Price = product.Price.Amount,
            Currency = product.Price.Currency.Code,
            //SKU = product.Sku.Value,
            CreatedDate = product.CreatedDate,
            ProductName = product.ProductName.Value,
            AllowDelivery = product.AllowDelivery,
            /*RestaurantId = product.RestaurantId.Value,
            RestaurantName = product?.Restaurant.RestaurantName.Value ?? "",*/
            ImageUrl = _awsS3Service.GetUrlPresign(product.ImageUrl.Value),
            CategoryResponse = new CategoryResponse()
            {
                Id = product.Category.Id.Value,
                CategoryName = product.Category.CategoryName.Value
            }
        });
    }
}