using Domain.Abstractions;
using Domain.Orders;
using Domain.Shared;

namespace Domain.Deliveries;

public class Delivery : Entity<DeliveryId>
{
    private Delivery(DeliveryId id, OrderId orderId, ReceiverName receiverName, PhoneNumber phoneNumber,
        ReceivingAddress receivingAddress, Note note, HasIssueAnInvoice hasIssueAnInvoice,
        CompanyTaxCode companyTaxCode, CompanyName companyName, CompanyEmail companyEmail,
        CompanyAddress companyAddress, HasRequestCutlery hasRequestCutlery) : base(id)
    {
        OrderId = orderId;
        ReceiverName = receiverName;
        PhoneNumber = phoneNumber;
        ReceivingAddress = receivingAddress;
        Note = note;
        HasIssueAnInvoice = hasIssueAnInvoice;
        CompanyTaxCode = companyTaxCode;
        CompanyName = companyName;
        CompanyEmail = companyEmail;
        CompanyAddress = companyAddress;
        HasRequestCutlery = hasRequestCutlery;
    }

    public ReceiverName ReceiverName { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public ReceivingAddress ReceivingAddress { get; private set; }
    public Note Note { get; private set; }
    public HasIssueAnInvoice HasIssueAnInvoice { get; private set; }
    public CompanyTaxCode CompanyTaxCode { get; private set; }
    public CompanyName CompanyName { get; private set; }
    public CompanyEmail CompanyEmail { get; private set; }
    public CompanyAddress CompanyAddress { get; private set; }
    public HasRequestCutlery HasRequestCutlery { get; private set; }
    public OrderId OrderId { get; private set; }

    public static Delivery Create(OrderId orderId, ReceiverName receiverName, PhoneNumber phoneNumber,
        ReceivingAddress receivingAddress, Note note, HasIssueAnInvoice hasIssueAnInvoice,
        CompanyTaxCode companyTaxCode, CompanyName companyName, CompanyEmail companyEmail,
        CompanyAddress companyAddress, HasRequestCutlery hasRequestCutlery)
    {
        return new Delivery(DeliveryId.New(), orderId, receiverName, phoneNumber, receivingAddress, note,
            hasIssueAnInvoice, companyTaxCode, companyName, companyEmail, companyAddress, hasRequestCutlery);
    }
}