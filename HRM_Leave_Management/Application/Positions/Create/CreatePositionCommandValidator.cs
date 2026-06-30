using FluentValidation;

namespace Application.Positions.Create;

internal sealed class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Position code is required.")
            .MaximumLength(20).WithMessage("Position code must not exceed 20 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Position name is required.")
            .MaximumLength(200).WithMessage("Position name must not exceed 200 characters.");

        RuleFor(x => x.Level)
            .GreaterThan(0).WithMessage("Position level must be greater than 0.");
    }
}
