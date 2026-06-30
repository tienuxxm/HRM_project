namespace Domain.PhoneValidationCheck;

public interface IPhoneValidationCheckRepository
{
    void Add(PhoneValidationCheck phoneValidationCheck);
    void Update(PhoneValidationCheck phoneValidationCheck);

    Task<PhoneValidationCheck?>
        GetByPhoneNumber(PhoneNumber phoneNumber, CancellationToken cancellationToken = default);
}