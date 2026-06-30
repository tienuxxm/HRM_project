using System.ComponentModel;

namespace Domain.Orders;

public enum OrderType
{
    [Description("Delivery")] Delivery,
    [Description("Reserve")] Booking,
}