using Application.Abstractions.Messaging;
using Application.MembershipClasses.Create;
using Domain.MembershipClasses;
using Domain.Shared;

namespace Application.MembershipClasses.Update;

public record UpdateMembershipClassCommand(
    MembershipClassId MembershipClassId,
    ClassName ClassName,
    Level Level,
    Money MaxMoney,
    List<MembershipBenefitRequestCommand> Benefits,
    float PercentDefault,
    float PercentBirthDate,
    int effectiveYears
) : ICommand;