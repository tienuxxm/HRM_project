namespace Application.Orders.GetRevenue;

public class RevenueResponse
{
    public int Value { get; set; }
    public bool IsCurrent { get; set; }
    public string Name { get; set; }
    public RevenueDataRangeType RevenueDataRangeType { get; set; }
}