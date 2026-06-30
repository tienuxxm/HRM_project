namespace Application.Orders.GetOrder;

public sealed class DeliveryResponse
{
    public string ReceiverName { get; set; }
    public string PhoneNumber { get; set; }
    public string ReceivingAddress { get; set; }
    public string Note { get; set; }
    public bool HasIssueAnInvoice { get; set; }
    public string? CompanyTaxCode { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyEmail { get; set; }
    public string? CompanyAddress { get; set; }
    public bool HasRequestCutlery { get; set; }
}