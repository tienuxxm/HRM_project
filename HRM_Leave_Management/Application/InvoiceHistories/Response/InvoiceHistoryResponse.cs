using Application.Extensions;
using Domain.Invoices;
using Domain.Orders;
using Domain.Shared;

namespace Application.InvoiceHistories.Response;

public class InvoiceHistoryResponse
{
    public string InvoiceCode { get; set; }
    public DateTime? PaymentDate { get; set; }
    public OrderType OrderType { get; set; }
    public string OrderTypeDisplay => OrderType.GetDescription();
    public PaymentType? PaymentType { get; set; }
    public string PaymentTypeDisplay => PaymentType?.GetDescription() ?? "";
    public int Quantity { get; set; }
    public Money TotalBill { get; set; }
    public string Title { get; set; }
    public string TotalBillDisplay => TotalBill.ToVndFormat();
}