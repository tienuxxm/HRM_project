using Application.Abstractions.Messaging;
using Domain.Orders;
using Domain.Products;
using Domain.Abstractions;
using Domain.Shared;

namespace Application.Orders.AddLineItem;

internal sealed class AddLineItemCommandHandler : ICommandHandler<AddLineItemCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddLineItemCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddLineItemCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(new OrderId(request.OrderId), cancellationToken);
        if (order is null)
        {
            return Result.Failure<Guid>(OrderErrors.NotFound);
        }

        var product = await _productRepository.GetByIdAsync(new ProductId(request.ProductId));
        if (product is null)
        {
            return Result.Failure<Guid>(ProductErrors.NotFound);
        }

        order.AddLineItem(product.Id, product.ProductName, product.Price, request.Amount, product.ImageUrl);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}