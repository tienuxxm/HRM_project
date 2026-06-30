using Domain.Abstractions;
using Domain.Invoices;

namespace Domain.PaymentDetails;

public sealed class PaymentDetail : Entity<PaymentDetailId>
{
    private PaymentDetail()
    {
    }

    private PaymentDetail(PaymentDetailId id, InvoiceId invoiceId, PaymentPlatform paymentPlatformm,
        TransactionRefId transactionRefId, PaymentResponse paymentResponse, DateTime paymentDate) : base(id)
    {
        InvoiceId = invoiceId;
        PaymentPlatform = paymentPlatformm;
        TransactionRefId = transactionRefId;
        PaymentResponse = paymentResponse;
        PaymentDate = paymentDate;
    }

    public static PaymentDetail Create(InvoiceId invoiceId, PaymentPlatform paymentPlatformm,
        TransactionRefId transactionRefId, PaymentResponse paymentResponse, DateTime paymentDate)
    {
        return new PaymentDetail(PaymentDetailId.New, invoiceId, paymentPlatformm, transactionRefId, paymentResponse,
            paymentDate);
    }

    public InvoiceId InvoiceId { get; private set; }
    public TransactionRefId TransactionRefId { get; private set; }
    public PaymentPlatform PaymentPlatform { get; private set; }
    public PaymentResponse PaymentResponse { get; private set; }
    public DateTime PaymentDate { get; private set; }
}