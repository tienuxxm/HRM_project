using Application.Abstractions.Messaging;
using Application.MembershipClasses.GetOne;

namespace Application.MembershipClasses.GetAll;

public sealed record GetAllMembershipClassCommand : ICommand<List<MembershipClassResponse>>;