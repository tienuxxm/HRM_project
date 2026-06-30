using System.Globalization;
using Application.Extensions;
using Domain.Invoices;
using Domain.Orders;
using Domain.Shared;

namespace Application.Orders.GetOrder;

public sealed class OrderResponse
{
    public Guid Id { get; init; }
    public string OrderIdFormat => $"\"{Id.ToString()}\"";
    public Guid MemberId { get; init; }
    public string? MemberAvatar { get; set; }
    public string? Note { get; init; }
    public OrderStatus Status { get; init; }
    public string StatusDisplay => Status.GetDescription();
    public string OrderCode { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? CompletedDate { get; init; }
    public Money TotalPrice { get; init; }
    public InvoiceResponse? InvoiceResponse { get; set; }

    public Money TotalProductPrice => LineItems.Aggregate(TotalPrice with { Amount = 0 },
        (s, d) => s + d.TotalPrice);

    public string TotalProductPriceDisplay =>
        TotalProductPrice.Amount.ToString("#,###", CultureInfo.GetCultureInfo("vi-VN").NumberFormat) + " " +
        TotalProductPrice.Currency.Code;

    public PaymentType? PaymentType { get; set; }
    public bool HasPayment { get; set; }
    public string? PaymentTypeDisplay => PaymentType?.GetDescription();

    public string TotalPriceDisplay =>
        TotalPrice.Amount.ToString("#,###", CultureInfo.GetCultureInfo("vi-VN").NumberFormat) + " " +
        TotalPrice.Currency.Code;

    public int TotalQuantity { get; init; }
    public List<LineItemResponse> LineItems { get; set; }
    public List<FeeResponse>? OrderFees { get; set; }
    public DeliveryResponse? Delivery { get; set; }

    public string OrderStatusDisplay => Status switch
    {
        OrderStatus.Created => "Created",
        OrderStatus.Process => "Processing",
        OrderStatus.Shipping => "Shipping",
        OrderStatus.Done => "Completed",
        OrderStatus.Cancel => "Canceled",
        _ => "--"
    };
}

public class InvoiceResponse
{
    public string InvoiceCode { get; set; }
    public DateTime? PaymentDate { get; set; }
    public int TotalQuantity { get; set; }
    public Money TotalBill { get; set; }
    public PaymentType? PaymentType { get; set; }
}

public class FeeResponse
{
    public string FeeName { get; set; }
    public string FeeValue { get; set; }
    public Money ChargeFee { get; set; }
    public bool IsPercent { get; set; }

    public string ChargeFeeDisplay =>
        ChargeFee.Amount.ToString("#,###", CultureInfo.GetCultureInfo("vi-VN").NumberFormat) + " " +
        ChargeFee.Currency.Code;
}