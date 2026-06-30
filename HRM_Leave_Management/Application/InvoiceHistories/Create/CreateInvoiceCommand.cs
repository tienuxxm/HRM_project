using Application.Abstractions.Messaging;

namespace Application.InvoiceHistories.Create;

public record CreateInvoiceCommand(
    GeneralInvoiceInfo GeneralInvoiceInfo,
    CustomerInfo CustomerInfo,
    List<ItemInfo> ItemInfos,
    SummarizeInfo SummarizeInfo,
    BranchInfo BranchInfo,
    List<Voucher> Vouchers
) : ICommand<InvoiceResponse>;

public record BranchInfo
{
    public string BranchLegalName { get; set; }
}

public record Voucher
{
    public Guid VoucherId { get; set; }
}

public record GeneralInvoiceInfo
{
    public int InvoiceType { get; set; }
    public Guid TransactionUuid { get; set; }
    public string? CurrencyCode { get; set; }
    public bool? PaymentStatus { get; set; }
    public string? InvoiceIssuedDate { get; set; }
}

public record CustomerInfo
{
    public string CustomerName { get; set; }
    public string CustomerPhoneNumber { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerAddressLine { get; set; }
}

public class InvoiceResponse
{
    public string Message { get; set; }
}

public class ItemInfo
{
    public string ItemName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public int TaxPercentage { get; set; }
    public int TaxAmount { get; set; }
}

public class SummarizeInfo
{
    public decimal SumOfTotalLineAmountWithoutTax { get; set; }
    public decimal TotalAmountWithoutTax { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalAmountWithTax { get; set; }
    public decimal TotalAmountWithTaxInWords { get; set; }
    public decimal DiscountAmount { get; set; }
    public int TaxPercentage { get; set; }
}