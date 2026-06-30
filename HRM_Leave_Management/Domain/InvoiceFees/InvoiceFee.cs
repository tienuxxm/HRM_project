using Domain.Abstractions;
using Domain.Invoices;
using Domain.Shared;

namespace Domain.InvoiceFees;

public class InvoiceFee : Entity<InvoiceFeeId>
{
    public InvoiceFeeName InvoiceFeeName { get; private set; }
    public InvoiceFeeAmount InvoiceFeeAmount { get; private set; }
    public InvoiceId InvoiceId { get; private set; }
    public Money FeeChange { get; private set; }
    public bool IsPercent { get; private set; }

    private InvoiceFee()
    {
    }

    private InvoiceFee(InvoiceFeeId id, InvoiceId invoiceId, InvoiceFeeAmount invoiceFeeAmount,
        InvoiceFeeName invoiceFeeName, Money feeChange, bool isPercent) : base(id)
    {
        InvoiceId = invoiceId;
        InvoiceFeeAmount = invoiceFeeAmount;
        InvoiceFeeName = invoiceFeeName;
        FeeChange = feeChange;
        IsPercent = isPercent;
    }

    public static InvoiceFee Create(InvoiceId invoiceId, InvoiceFeeAmount invoiceFeeAmount,
        InvoiceFeeName invoiceFeeName, Money feeCharge, bool isPercent)
    {
        return new InvoiceFee(InvoiceFeeId.New, invoiceId, invoiceFeeAmount, invoiceFeeName, feeCharge, isPercent);
    }
}