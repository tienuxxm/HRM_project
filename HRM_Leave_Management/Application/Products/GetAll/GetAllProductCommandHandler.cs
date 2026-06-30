using Application.Abstractions.Messaging;
using Application.Products.GetOne;
using Domain.Abstractions;
using Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Application.Products.GetAll;

internal sealed class GetAllProductCommandHandler : ICommandHandler<GetAllProductCommand, List<ProductResponse>>
{
    private readonly IProductRepository _productRepository;

    public GetAllProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<List<ProductResponse>>> Handle(GetAllProductCommand request,
        CancellationToken cancellationToken)
    {
        var query = _productRepository.GetEntitiesAsQueryable();
        if (request.allowDelivery)
        {
            query = query.Where(x => x.AllowDelivery).AsQueryable();
        }

        var result = await query.ToListAsync(cancellationToken);
        var productsResponse = result.Select(p => new ProductResponse
        {
            Id = p.Id.Value,
            //SKU = p.Sku.Value,
            Currency = p.Price.Currency.Code,
            Price = p.Price.Amount,
            ProductName = p.ProductName.Value,
            AllowDelivery = p.AllowDelivery,
        }).ToList();
        return Result.Success(productsResponse);
    }
}