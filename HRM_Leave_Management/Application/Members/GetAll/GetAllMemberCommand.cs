using Application.Abstractions.Messaging;
using Application.Members.Responses;

namespace Application.Members.GetAll;

public record GetAllMemberCommand() : ICommand<List<MemberResponse>>;