using Application.Orders.GetOrder;
using Domain.Orders;

namespace Web.Backend.Models;

public class OrderAndDeliveryDetailViewModel
{
    public OrderResponse Order { get; set; }

    public Dictionary<int, string> UpdateOrderStatuses => Order.Status switch
    {
        OrderStatus.Process => new Dictionary<int, string>
        {
            { (int)OrderStatus.Shipping, "Shipping" }, { (int)OrderStatus.Done, "Completed" }
        },
        OrderStatus.Shipping => new Dictionary<int, string>
        {
            { (int)OrderStatus.Done, "Completed" }
        },
        _ => new Dictionary<int, string>()
    };
}