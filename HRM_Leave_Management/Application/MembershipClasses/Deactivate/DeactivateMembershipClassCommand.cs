using Application.Abstractions.Messaging;
using Domain.MembershipClasses;

namespace Application.MembershipClasses.Deactivate;

public record DeactivateMembershipClassCommand(MembershipClassId Id) : ICommand;