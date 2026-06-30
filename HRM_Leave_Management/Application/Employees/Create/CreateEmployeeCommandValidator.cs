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
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.JoinDate)
            .NotEmpty();
    }
}
