using Application.Products.GetOne;
using Domain.Invoices;

namespace Web.Backend.Models;

public class ManageOrderViewModel
{
    public string Title { get; set; } = "Add Order";
    public OrderViewModel Model { get; set; }

    public Dictionary<int, string> PatymentTypes => Enum.GetValues(typeof(PaymentType)).Cast<PaymentType>()
        .ToDictionary(t => (int)t, v => v switch
        {
            PaymentType.Banking => "Ngân hàng",
            PaymentType.Cash => "Cash",
            _ => string.Empty
        });

    public List<ProductResponse> Products { get; set; }
}

public class OrderViewModel
{
    public Guid? Id { get; set; }
    public Guid MemberId { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string Fullname { get; set; } = string.Empty;
    public string? Note { get; set; } = string.Empty;
    public bool HasIssueAnInvoice { get; set; }
    public bool HasRequestCutlery { get; set; }
    public PaymentType PaymentType { get; set; }
    public string? CompanyTaxCode { get; set; } = string.Empty;
    public string? CompanyName { get; set; } = string.Empty;
    public string? CompanyEmail { get; set; } = string.Empty;
    public string? CompanyAddress { get; set; } = string.Empty;
    public List<OrderLineItemModel> LineItems { get; set; }
}

public class OrderLineItemModel
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
}