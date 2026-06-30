using Application.Abstractions.Messaging;
using Domain.MembershipClasses;

namespace Application.MembershipClasses.GetOne;

public record GetOneMembershipClassCommand(MembershipClassId Id) : ICommand<MembershipClassResponse>;