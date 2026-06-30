using Application.Abstractions.Messaging;
using Application.Members.Responses;
using Domain.Abstractions;
using Domain.Members;

namespace Application.Members.GetAllPaged;

public record GetAllMemberPagedCommand() : PagedQuery<Member, MemberId>, ICommand<PagedList<MemberResponse>>;