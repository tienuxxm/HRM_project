using FluentValidation;

namespace Application.LeaveApproverAssignments.Update;

internal sealed class UpdateLeaveApproverAssignmentCommandValidator : AbstractValidator<UpdateLeaveApproverAssignmentCommand>
{
    public UpdateLeaveApproverAssignmentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Assignment ID is required.");

        RuleFor(x => x.ApproverEmployeeId)
            .NotEmpty().WithMessage("Approver employee is required.");

        RuleFor(x => x)
            .Must(x => !x.EffectiveFrom.HasValue || !x.EffectiveTo.HasValue || x.EffectiveFrom.Value <= x.EffectiveTo.Value)
            .WithMessage("Effective from date must be before or equal to effective to date.")
            .WithName("EffectiveDates");
    }
}
