using FluentValidation;

namespace Application.Sms.SendValidationCode;

internal sealed class SendValidationCodeCommandValidator : AbstractValidator<SendValidationCodeCommand>
{
    public SendValidationCodeCommandValidator()
    {
        RuleFor(c => c.PhoneNumber).NotEmpty();
    }
}