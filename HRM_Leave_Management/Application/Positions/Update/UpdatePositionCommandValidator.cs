using FluentValidation;

namespace Application.Positions.Update;

internal sealed class UpdatePositionCommandValidator : AbstractValidator<UpdatePositionCommand>
{
    public UpdatePositionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Position identifier is required.");

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
