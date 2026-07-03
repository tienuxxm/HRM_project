using FluentValidation;

namespace Application.LeaveApproverAssignments.Create;

internal sealed class CreateLeaveApproverAssignmentCommandValidator : AbstractValidator<CreateLeaveApproverAssignmentCommand>
{
    public CreateLeaveApproverAssignmentCommandValidator()
    {
        RuleFor(x => x.ApproverEmployeeId)
            .NotEmpty().WithMessage("Approver employee is required.");

        RuleFor(x => x)
            .Must(x => !x.EffectiveFrom.HasValue || !x.EffectiveTo.HasValue || x.EffectiveFrom.Value <= x.EffectiveTo.Value)
            .WithMessage("Effective from date must be before or equal to effective to date.")
            .WithName("EffectiveDates");
    }
}
