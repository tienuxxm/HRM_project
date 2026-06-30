using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Application.InvoiceHistories.Cancel;

public class CancelInvoiceCommandHandler : ICommandHandler<CancelInvoiceCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelInvoiceCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelInvoiceCommand request, CancellationToken cancellationToken)
    {
        var orders = (await _orderRepository.GetEntitiesAsQueryable()
            .Where(x => x.OrderRef != null && x.Status != OrderStatus.Cancel)
            .ToListAsync(cancellationToken)).Where(x => request.Transactions.Any(t => t == x.OrderRef)).ToList();

        if (!orders.Any()) return Result.Failure(new Error("NOT_FOUND", "NO INVOICE FOUND"));
        using var trx = _unitOfWork.BeginTransaction();
        try
        {
            orders.ForEach(order => order.CancelOrder());
            _orderRepository.UpdateRange(orders);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            trx.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            trx.Rollback();
            return Result.Failure(new Error("EXCEPTION", ex.Message));
        }
    }
}