using Application.Abstractions.Messaging;
using Application.Orders.GetOrder;
using Domain.Abstractions;
using Domain.Orders;

namespace Application.Orders.GetAllPaged;

public record GetAllOrderPagedCommand() : PagedQuery<Order, OrderId>, ICommand<PagedList<OrderResponse>>;