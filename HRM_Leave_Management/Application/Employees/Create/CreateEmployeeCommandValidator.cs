using FluentValidation;

namespace Application.Employees.Create;

internal sealed class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.EmployeeCode)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.EmployeeCode));

        RuleFor(x => x.JoinDate)
            .NotEmpty();
    }
}
