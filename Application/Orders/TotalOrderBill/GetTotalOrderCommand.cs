using Application.Abstractions.Messaging;
using Application.Orders.Create;
using Application.Orders.Response;
using Domain.Orders;

namespace Application.Orders.TotalOrderBill;

public record GetTotalOrderCommand() : ICommand<TotalOrderResponse>
{
    public List<CreateLineItem> LineItems { get; set; }
    public OrderType OrderType { get; set; }
}