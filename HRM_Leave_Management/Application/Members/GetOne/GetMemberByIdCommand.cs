using Application.Abstractions.Messaging;
using Application.Members.Responses;
using Domain.Members;

namespace Application.Members.GetOne;

public record GetMemberByIdCommand(MemberId MemberId) : ICommand<MemberResponse>;