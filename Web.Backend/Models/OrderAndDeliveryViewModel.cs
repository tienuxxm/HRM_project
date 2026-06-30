using Application.Orders.GetOrder;
using Domain.Abstractions;

namespace Web.Backend.Models;

public class OrderAndDeliveryViewModel
{
    public PagedList<OrderResponse> Response { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortColumn { get; set; }
    public string? SortOrder { get; set; }
}