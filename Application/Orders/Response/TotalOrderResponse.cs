namespace Application.Orders.Response;

public class TotalOrderResponse
{
    public Guid OrderId { get; set; }
    public string TotalBill { get; set; }
    public List<OrderFeeResponse>? OrderFeeResponses { get; set; }
}

public class OrderFeeResponse
{
    public string FeeName { get; set; }
    public string FeeValue { get; set; }
}