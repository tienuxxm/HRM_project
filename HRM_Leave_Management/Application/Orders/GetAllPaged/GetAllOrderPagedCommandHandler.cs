using Application.Abstractions.Messaging;
using Application.Orders.GetOrder;
using Domain.Abstractions;
using Domain.Orders;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.GetAllPaged;

internal sealed class
    GetAllOrderPagedCommandHandler : ICommandHandler<GetAllOrderPagedCommand, PagedList<OrderResponse>>
{
    private readonly IOrderRepository _orderRepository;

    public GetAllOrderPagedCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<PagedList<OrderResponse>>> Handle(GetAllOrderPagedCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = _orderRepository.GetEntitiesAsQueryable()
                .Where(x => x.OrderType != OrderType.Booking && x.Status != OrderStatus.Draft)
                .Include(x => x.LineItems)
                .Include(x => x.Member)
                .OrderByDescending(x => x.CreatedDate)
                .Include(x => x.Delivery).AsQueryable();
            if (!string.IsNullOrEmpty(request.SearchTerm))
                query = query.AsEnumerable()
                    .Where(x => x.Member.FullName.ToLower().Contains(request.SearchTerm.ToLower()) ||
                                x.Member.PhoneNumber.Value.ToLower().Contains(request.SearchTerm.ToLower()) ||
                                x.OrderCode.Value.ToLower().Contains(request.SearchTerm.ToLower()))
                    .AsQueryable();

            if (request.SortColumn == "TotalQuantity")
            {
                request.SortColumn = "";
                query = request.SortOrder == "ASC"
                    ? query.OrderBy(x => x.LineItems.Count)
                    : query.OrderByDescending(x => x.LineItems.Count);
            }

            if (request.SortColumn == nameof(Order.TotalBill))
            {
                request.SortColumn = "";
                query = request.SortOrder == "ASC"
                    ? query.AsEnumerable().OrderBy(x => x.TotalBill.Amount).AsQueryable()
                    : query.AsEnumerable().OrderByDescending(x => x.TotalBill.Amount).AsQueryable();
            }

            var result = await _orderRepository.GetAllPaged(request, query);
            var orders = result.Data.Select(order => new OrderResponse
            {
                Id = order.Id.Value,
                Note = order.Note?.Value,
                Status = order.Status,
                CompletedDate = order.CompletedDate,
                HasPayment = order.HasPayment,
                CreatedDate = order.CreatedDate,
                MemberId = order.MemberId.Value,
                OrderCode = order.OrderCode.Value,
                PaymentType = order.PaymentType,
                TotalPrice = order.LineItems.Count > 0
                    ? new Money(order.LineItems.Sum(x => x.Quantity * x.Price.Amount),
                        order.LineItems.First().Price.Currency)
                    : Money.Zero(),
                TotalQuantity = order.LineItems.Count > 0 ? order.LineItems.Sum(x => x.Quantity) : 0,
                Delivery = order.Delivery != null
                    ? new DeliveryResponse
                    {
                        Note = order.Delivery.Note.Value,
                        CompanyAddress = order.Delivery.CompanyAddress.Value,
                        CompanyEmail = order.Delivery.CompanyEmail.Value,
                        CompanyName = order.Delivery.CompanyName.Value,
                        CompanyTaxCode = order.Delivery.CompanyTaxCode.Value,
                        PhoneNumber = order.Delivery.PhoneNumber.Value,
                        ReceiverName = order.Delivery.ReceiverName.Value,
                        ReceivingAddress = order.Delivery.ReceivingAddress.Value,
                        HasRequestCutlery = order.Delivery.HasRequestCutlery.Value,
                        HasIssueAnInvoice = order.Delivery.HasIssueAnInvoice.Value
                    }
                    : null,
                LineItems = order.LineItems.Select(l => new LineItemResponse
                {
                    Id = l.Id.Value,
                    Price = l.Price,
                    Quantity = l.Quantity,
                    ProductId = l.ProductId.Value,
                    OrderId = l.OrderId.Value,
                    ProductName = l.ProductName.Value
                }).ToList()
            }).ToList();
            return Result.Success(new PagedList<OrderResponse>(orders, result.TotalCount,
                result.CurrentPage, result.PageSize));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}