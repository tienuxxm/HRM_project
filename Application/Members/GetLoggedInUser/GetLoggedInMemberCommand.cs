using Application.Abstractions.Messaging;

namespace Application.Members.Responses;

public record GetLoggedInMemberCommand : ICommand<MemberResponse>;