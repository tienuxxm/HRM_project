using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Categories;
using Domain.Products;
using Domain.Shared;

namespace Application.Products.Update;

internal sealed class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(IProductRepository productRepository, ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product is null)
        {
            return Result.Failure<Guid>(ProductErrors.NotFound);
        }

        if (request.CategoryId is not null)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category is null)
                return Result.Failure<Guid>(CategoryErrors.NotFound);
        }

        product.Update(string.IsNullOrEmpty(request.Name) ? null : new ProductName(request.Name),
            request.Amount.HasValue && !string.IsNullOrEmpty(request.Currency)
                ? new Money(request.Amount.Value, Currency.FromCode(request.Currency))
                : null,
            request.CategoryId,
            string.IsNullOrEmpty(request.ImageKey) ? null : new ImageUrl(request.ImageKey),
            request.allowDelivery
        );

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}