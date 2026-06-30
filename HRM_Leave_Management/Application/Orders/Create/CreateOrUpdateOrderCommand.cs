using Application.Abstractions.Messaging;
using Application.Orders.Response;
using Domain.Bookings;
using Domain.Deliveries;
using Domain.Invoices;
using Domain.Orders;
using Domain.Products;
using Domain.Shared;
using Note = Domain.Deliveries.Note;

namespace Application.Orders.Create;

public record CreateOrUpdateOrderCommand(Guid? MemberId,
    string? Note,
    List<CreateLineItem>? LineItems, CreateDelivery? Delivery, PaymentType? PaymentType,
    Booking? Booking,
    OrderType OrderType = OrderType.Booking, Guid? OrderId = null) : ICommand<TotalOrderResponse>;

public sealed record CreateLineItem()
{
    public ProductId ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
};

public sealed record CreateDelivery(ReceiverName ReceiverName, PhoneNumber PhoneNumber,
    ReceivingAddress ReceivingAddress, Note Note, HasIssueAnInvoice HasIssueAnInvoice,
    CompanyTaxCode CompanyTaxCode, CompanyName CompanyName, CompanyEmail CompanyEmail,
    CompanyAddress CompanyAddress, HasRequestCutlery HasRequestCutlery);