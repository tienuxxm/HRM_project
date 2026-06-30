using Domain.Abstractions;
using Domain.Invoices;
using Domain.Products;
using Domain.Shared;

namespace Domain.InvoiceDetails;

public class InvoiceDetail : Entity<InvoiceDetailId>
{
    private InvoiceDetail()
    {
    }

    private InvoiceDetail(InvoiceDetailId id, InvoiceId invoiceId, ProductName productName, Money price,
        int quantity) : base(id)
    {
        InvoiceId = invoiceId;
        ProductName = productName;
        Price = price;
        Quantity = quantity;
    }

    public static InvoiceDetail Create(InvoiceId invoiceId, ProductName productName, Money price,
        int quantity)
    {
        return new InvoiceDetail(InvoiceDetailId.New, invoiceId, productName, price, quantity);
    }


    public InvoiceId InvoiceId { get; private set; }
    public ProductName ProductName { get; private set; }
    public Money Price { get; private set; }
    public int Quantity { get; private set; }
}