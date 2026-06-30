using Application.Abstractions.Messaging;
using Domain.MemberActivities;

namespace Application.MemberActivites;

public record CreateManyMemberActivityCommand(List<CreateMemberActivity> Activities) : ICommand;

public record CreateMemberActivity(string Message, MemberActivityType Type);