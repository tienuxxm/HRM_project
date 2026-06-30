namespace Domain.PaymentDetails;

public record PaymentDetailId(Guid Value)
{
    public static PaymentDetailId New => new PaymentDetailId(Guid.NewGuid());
}