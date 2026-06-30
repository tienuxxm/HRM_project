namespace Domain.InvoiceDetails;

public record InvoiceDetailId(Guid Value)
{
    public static InvoiceDetailId New => new InvoiceDetailId(Guid.NewGuid());
}