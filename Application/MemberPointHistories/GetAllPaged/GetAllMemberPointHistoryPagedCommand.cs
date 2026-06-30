using Application.Abstractions.Messaging;
using Application.MemberPointHistories.Response;
using Domain.Abstractions;
using Domain.MemberPointHistories;
using Domain.Members;

namespace Application.MemberPointHistories.GetAllPaged;

public record GetAllMemberPointHistoryPagedCommand(MemberId? MemberId, PointType? PointType = null) :
    PagedQuery<MemberPointHistory, MemberPointHistoryId>,
    ICommand<PagedList<MemberPointHistoryResponse>>
{
}