using Application.Abstractions.Messaging;
using Domain.MembershipClasses;
using Domain.Shared;

namespace Application.MembershipBenefits.AddRange;

public record AddRangeMembershipBenefitCommand() : ICommand
{
    public MembershipClassId MembershipClassId { get; set; }
    public List<AddMembershipBenefit> AddMembershipBenefits { get; set; }
}

public record AddMembershipBenefit(Title Title, Description Description);