using System.Globalization;
using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Categories.GetOne;
using Application.Products.GetOne;
using Domain.Abstractions;
using Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Application.Products.GetAllPaged;

internal sealed class GetProductsCommandHandler : ICommandHandler<GetProductsCommand, GetProductsResponse>
{
    private readonly IAwsS3Service _awsS3Service;
    private readonly IProductRepository _productRepository;

    public GetProductsCommandHandler(IProductRepository productRepository, IAwsS3Service awsS3Service)
    {
        _productRepository = productRepository;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<GetProductsResponse>> Handle(GetProductsCommand request,
        CancellationToken cancellationToken)
    {
        var query = _productRepository.GetEntitiesAsQueryable()
            .Include(x => x.Category).Where(x => x.IsDeleted == null || x.IsDeleted == false).AsQueryable();
        if (request.AllowDelivery.HasValue && request.AllowDelivery.Value) query = query.Where(x => x.AllowDelivery);

        if (request.SortColumn is "CategoryName")
        {
            query = request.SortOrder == "ASC"
                ? query.OrderBy(x => x.Category.CategoryName)
                : query.OrderByDescending(x => x.Category.CategoryName);
            request.SortOrder = "";
        }

        if (request.SortColumn is "Price")
        {
            query = request.SortOrder == "ASC"
                ? query.AsEnumerable().OrderBy(x => x.Price.Amount).AsQueryable()
                : query.AsEnumerable().OrderByDescending(x => x.Price.Amount).AsQueryable();
            request.SortOrder = "";
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
            query = query.AsEnumerable()
                .Where(x => x.ProductName.Value.ToLower().Contains(request.SearchTerm.ToLower())
                            || x.Category.CategoryName.Value.ToLower().Contains(request.SearchTerm.ToLower())
                            || x.Price.Amount.ToString(CultureInfo.InvariantCulture) == request.SearchTerm)
                .AsQueryable();

        var result = await _productRepository.GetAllPaged(request, query);
        var resultDto = result.Data.Select(p => new ProductResponse
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
            ImageUrl = _awsS3Service.GetUrlPresign(p.ImageUrl.Value) ?? ""
        }).ToList();
        var resultList =
            new GetProductsResponse(resultDto, result.TotalCount, result.CurrentPage, result.PageSize);
        return Result.Success(resultList);
    }
}