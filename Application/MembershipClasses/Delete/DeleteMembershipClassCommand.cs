using Application.Abstractions.Messaging;
using Domain.MembershipClasses;

namespace Application.MembershipClasses.Delete;

public record DeleteMembershipClassCommand(MembershipClassId Id) : ICommand;