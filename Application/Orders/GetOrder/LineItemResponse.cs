using System.Globalization;
using Domain.Deliveries;
using Domain.Shared;

namespace Application.Orders.GetOrder;

public sealed class LineItemResponse
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public string ProductName { get; init; }
    public Guid ProductId { get; init; }
    public Money Price { get; init; }
    public string? ProductImage { get; set; }
    public string? Note { get; set; }
    public Money TotalPrice => Price with { Amount = Price.Amount * Quantity };

    public string PriceDisplay => Price.Amount.ToString("#,###", CultureInfo.GetCultureInfo("vi-VN").NumberFormat) +
                                  " " + Price.Currency.Code;

    public string TotalPriceDisplay =>
        TotalPrice.Amount.ToString("#,###", CultureInfo.GetCultureInfo("vi-VN").NumberFormat) +
        " " + Price.Currency.Code;

    public int Quantity { get; init; }
}