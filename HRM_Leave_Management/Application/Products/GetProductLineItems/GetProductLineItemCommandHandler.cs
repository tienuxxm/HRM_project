/*using Application.Abstractions.Messaging;
using Application.Orders.Create;
using Domain.Abstractions;
using Domain.Products;

namespace Application.Products.GetProductLineItems;

public class GetProductLineItemCommandHandler : ICommandHandler<GetProductLineItemCommand, List<CreateLineItem>>
{
    private readonly IProductRepository _productRepository;

    public GetProductLineItemCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<List<CreateLineItem>>> Handle(GetProductLineItemCommand request,
        CancellationToken cancellationToken)
    {
        var productsIds = request.ListProductsLineItemRequests.Select(x => new ProductId(x.ProductId)).ToList();
        var products = await _productRepository.GetByIdsAsync(productsIds, cancellationToken);
        if (products is null)
            return Result.Failure<List<CreateLineItem>>(ProductErrors.NotFound);
        var lineItemResponse = products.Select(p => new CreateLineItem()
        {
            Price = p.Price,
            Quantity = request.ListProductsLineItemRequests.First(r => r.ProductId == p.Id.Value).Quantity,
            ProductId = p.Id,
            ProductName = p.ProductName
        }).ToList();
        return Result.Success(lineItemResponse);
    }
}*/

