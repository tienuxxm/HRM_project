using FluentValidation;

namespace Application.Employees.ProvisionAccount;

internal sealed class ProvisionEmployeeAccountCommandValidator : AbstractValidator<ProvisionEmployeeAccountCommand>
{
    public ProvisionEmployeeAccountCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty();

        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(150);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);

        RuleFor(x => x.RoleIds)
            .NotEmpty()
            .WithMessage("Danh sách vai trò không được để trống.")
            .Must(roleIds => roleIds == null || roleIds.Distinct().Count() == roleIds.Count)
            .WithMessage("Danh sách vai trò không được chứa các giá trị trùng lặp.");
    }
}
