using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Application.Abstractions.Sms;
using Domain.Abstractions;
using Domain.PhoneValidationCheck;

namespace Application.Sms.ValidateCode;

public class ValidateSmsCodeCommandHandler : ICommandHandler<ValidateSmsCodeCommand>
{
    private readonly IPhoneValidationCheckRepository _phoneValidationCheckRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ValidateSmsCodeCommandHandler(ISmsService smsService,
        IPhoneValidationCheckRepository phoneValidationCheckRepository, IDateTimeProvider dateTimeProvider)
    {
        _phoneValidationCheckRepository = phoneValidationCheckRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(ValidateSmsCodeCommand request, CancellationToken cancellationToken)
    {
        var phoneValidate = await _phoneValidationCheckRepository.GetByPhoneNumber(
            new PhoneNumber(request.PhoneNumber.Value), cancellationToken);
        if (phoneValidate is null)
            return Result.Failure(new Error("CodeValidation.Fail", "Fail to validate code"));
        var isCodeValid = phoneValidate.IsCodeValid(_dateTimeProvider.UtcNow, new Code(request.Code));
        return isCodeValid;
    }
}