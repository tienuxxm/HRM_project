using FluentValidation;

namespace Application.Members.RegisterMember;

internal sealed class RegisterMemberCommandValidator : AbstractValidator<RegisterMemberCommand>
{
    public RegisterMemberCommandValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty();

        RuleFor(c => c.LastName).NotEmpty();
        RuleFor(c => c.PhoneNumber).NotEmpty();

        RuleFor(c => c.Email).EmailAddress();

        RuleFor(c => c.Password).NotEmpty().MinimumLength(5);
    }
}