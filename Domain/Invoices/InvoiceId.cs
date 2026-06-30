namespace Domain.Invoices;

public record InvoiceId(Guid Value)
{
    public static InvoiceId New => new InvoiceId(Guid.NewGuid());
};