namespace Domain.InvoiceFees;

public record InvoiceFeeId(Guid Value)
{
    public static InvoiceFeeId New => new InvoiceFeeId(Guid.NewGuid());
}