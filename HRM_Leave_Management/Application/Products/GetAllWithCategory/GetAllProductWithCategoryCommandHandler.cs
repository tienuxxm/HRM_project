using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Categories.GetOne;
using Application.Products.GetOne;
using Domain.Abstractions;
using Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Application.Products.GetAllWithCategory;

public class
    GetAllProductWithCategoryCommandHandler : ICommandHandler<GetAllProductWithCategoryCommand,
    List<ProductWithCategoryResponse>>
{
    private readonly IAwsS3Service _awsS3Service;
    private readonly IProductRepository _productRepository;

    public GetAllProductWithCategoryCommandHandler(IProductRepository productRepository, IAwsS3Service awsS3Service)
    {
        _productRepository = productRepository;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<List<ProductWithCategoryResponse>>> Handle(GetAllProductWithCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var query = _productRepository
            .GetEntitiesAsQueryable()
            .Where(x => !x.IsDeleted.HasValue || (x.IsDeleted.HasValue && !x.IsDeleted.Value)).AsQueryable();
        if (request.allowDelivery.HasValue)
            query = query.Where(x =>
                    x.AllowDelivery == request.allowDelivery.Value)
                .AsQueryable();

        var products = (await query
                .Include(x => x.Category)
                .OrderBy(x => x.Category.Index)
                .ThenBy(x => x.Category.CategoryName)
                .ToListAsync(cancellationToken))
            .GroupBy(x => new { x.Category.CategoryName })
            .Select(x => new ProductWithCategoryResponse
            {
                CategoryName = x.Key.CategoryName.Value,
                Products = query
                    .Include(k => k.Category)
                    .Where(y => y.Category.CategoryName.Equals(x.Key.CategoryName))
                    .Select(p => new ProductResponse
                    {
                        Id = p.Id.Value,
                        Price = p.Price.Amount,
                        Currency = p.Price.Currency.Code,
                        //SKU = p.Sku.Value,
                        AllowDelivery = p.AllowDelivery,
                        CreatedDate = p.CreatedDate,
                        ProductName = p.ProductName.Value,
                        CategoryResponse = new CategoryResponse
                        {
                            Id = p.Category.Id.Value,
                            CategoryName = p.Category.CategoryName.Value
                        },
                        ImageUrl = _awsS3Service.GetUrlPresign(p.ImageUrl.Value, 60)
                    }).ToList()
            }).ToList();

        return Result.Success(products);
    }
}