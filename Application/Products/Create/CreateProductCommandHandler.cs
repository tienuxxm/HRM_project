using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Categories;
using Domain.Products;
using Domain.Restaurants;
using Domain.Shared;

namespace Application.Products.Create;

internal class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IRestaurantRepository _restaurantRepository;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        IDateTimeProvider dateTimeProvider,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork, IRestaurantRepository restaurantRepository)
    {
        _productRepository = productRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
        _restaurantRepository = restaurantRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(new CategoryId(request.CategoryId), cancellationToken);
        /*var restaurant =
            await _restaurantRepository.GetByIdAsync(new RestaurantId(request.RestaurantId), cancellationToken);*/

        if (category is null)
        {
            return Result.Failure<Guid>(CategoryErrors.NotFound);
        }

        /*if (restaurant is null)
        {
            return Result.Failure<Guid>(RestaurantErrors.NotFound);
        }*/

        var product = Product.Create(
            category.Id,
            new ProductName(request.Name),
            new Money(request.Price, Currency.FromCode(request.Currency)),
            new ImageUrl(request.ImageUrl),
            _dateTimeProvider.UtcNow,
            request.allowDelivery);
        _productRepository.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return product.Id.Value;
    }
}