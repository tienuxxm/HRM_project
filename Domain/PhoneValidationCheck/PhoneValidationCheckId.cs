namespace Domain.PhoneValidationCheck;

public record PhoneValidationCheckId(Guid Value)
{
    public static PhoneValidationCheckId New => new PhoneValidationCheckId(Guid.NewGuid());
}