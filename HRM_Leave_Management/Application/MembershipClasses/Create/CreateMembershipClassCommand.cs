using Application.Abstractions.Messaging;
using Domain.MembershipClasses;
using Domain.Shared;

namespace Application.MembershipClasses.Create;

public record CreateMembershipClassCommand
(ClassName ClassName, Level Level, Money MaxMoney, List<MembershipBenefitRequestCommand> Benefits,
    float PercentDefault, float PercentBirthDate, int effectiveYears) : ICommand<Guid>;

public record MembershipBenefitRequestCommand
{
    public Title Title { get; set; }
    public Description? Description { get; set; }
}