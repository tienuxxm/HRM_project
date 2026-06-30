using Application.Abstractions.Messaging;
using Application.Orders.GetOrder;
using Domain.Abstractions;
using Domain.Orders;

namespace Application.Orders.MyOrder;

public record GetMyOrderCommand() : PagedQuery<Order, OrderId>, ICommand<PagedList<OrderResponse>>;