using System.ComponentModel;

namespace Domain.Orders;

public enum OrderStatus
{
    [Description("Created")] Created = 0,
    [Description("Processing")] Process = 2,
    [Description("Shipping")] Shipping = 3,
    [Description("Completed")] Done = 4,
    [Description("Canceled")] Cancel = 5,
    [Description("Draft")] Draft = 6
}