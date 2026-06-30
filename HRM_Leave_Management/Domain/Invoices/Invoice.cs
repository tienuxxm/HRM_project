using Domain.Abstractions;
using Domain.InvoiceDetails;
using Domain.InvoiceFees;
using Domain.Orders;
using Domain.PaymentDetails;
using Domain.Shared;

namespace Domain.Invoices;

public class Invoice : Entity<InvoiceId>
{
    private Invoice()
    {
    }

    private Invoice(InvoiceId id, OrderId orderId, Code invoiceCode, DateTime? paymentDate, PaymentType? paymentType,
        OrderType orderType, int totalQuantity, Money totalBill, Title title) : base(id)
    {
        OrderId = orderId;
        InvoiceCode = invoiceCode;
        PaymentDate = paymentDate;
        OrderType = orderType;
        TotalQuantity = totalQuantity;
        TotalBill = totalBill;
        PaymentType = paymentType;
        Title = title;
    }

    public PaymentDetail PaymentDetail { get; private set; }

    public void SetPaymentDetail(PaymentDetail paymentDetail)
    {
        PaymentDetail = paymentDetail;
    }

    public static Invoice Create(OrderId orderId, Code invoiceCode, DateTime? paymentDate, PaymentType? paymentType,
        OrderType orderType, int totalQuantity, Money totalBill, Title title)
    {
        return new Invoice(InvoiceId.New, orderId, invoiceCode, paymentDate, paymentType, orderType, totalQuantity,
            totalBill, title);
    }

    public void SetInvoiceDetail(List<InvoiceDetail> invoiceDetails)
    {
        InvoiceDetails = invoiceDetails;
    }

    public void SetInvoiceFee(List<InvoiceFee> invoiceFees)
    {
        InvoiceFees = invoiceFees;
    }

    public void Delete(DateTime dateTime)
    {
        IsDeleted = true;
        DeletedAt = dateTime;
    }

    public List<InvoiceFee> InvoiceFees { get; private set; }
    public List<InvoiceDetail> InvoiceDetails { get; private set; }

    public Code InvoiceCode { get; private set; }
    public DateTime? PaymentDate { get; private set; }
    public PaymentType? PaymentType { get; private set; }
    public OrderType OrderType { get; private set; }
    public int TotalQuantity { get; private set; }
    public Money TotalBill { get; private set; }
    public OrderId? OrderId { get; private set; }
    public Title Title { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
}