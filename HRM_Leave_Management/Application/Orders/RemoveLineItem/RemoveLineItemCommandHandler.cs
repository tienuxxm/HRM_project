using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Orders;


namespace Application.Orders.RemoveLineItem;

internal sealed class RemoveLineItemCommandHandler : ICommandHandler<RemoveLineItemCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveLineItemCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }


    public async Task<Result> Handle(RemoveLineItemCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetOrderIncludeLineItemByIdAsync(new OrderId(request.OrderId),
            cancellationToken);
        if (order is null)
            return Result.Failure(OrderErrors.NotFound);

        var lineItem = order.LineItems.FirstOrDefault(x => x.Id.Value == request.OrderId);
        if (lineItem is null)
            return Result.Failure(LineItemErrors.NotFound);

        order.RemoveLineItem(new LineItemId(request.LineId));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}