namespace Application.Orders.ConfirmPayment;

public class ConfirmPaymentResponse
{
    public string? TerminalId { get; set; }
    public string? OrderId { get; set; }
    public string? TransactionId { get; set; }
    public long? Amount { get; set; }
    public string? CurrencyCode { get; set; }
    public string? BankCode { get; set; }
    public bool HasPayment { get; set; } = false;
    public DateTime TransactionDate { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
}