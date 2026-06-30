using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.MemberPointHistories.Response;
using Application.MemberVoucher.Response;
using Domain.Abstractions;
using Domain.MemberPointHistories;
using Domain.Members;

namespace Application.MemberPointHistories.GetAllPaged;

internal sealed class GetAllMemberPointHistoryPagedCommandHandler : ICommandHandler<GetAllMemberPointHistoryPagedCommand
    ,
    PagedList<MemberPointHistoryResponse>>
{
    private readonly IMemberPointHistoryRepository _memberPointHistoryRepository;
    private readonly IMemberContext _memberContext;
    private readonly IMemberRepository _memberRepository;

    public GetAllMemberPointHistoryPagedCommandHandler(IMemberPointHistoryRepository memberPointHistoryRepository,
        IMemberContext memberContext, IMemberRepository memberRepository)
    {
        _memberPointHistoryRepository = memberPointHistoryRepository;
        _memberContext = memberContext;
        _memberRepository = memberRepository;
    }

    public async Task<Result<PagedList<MemberPointHistoryResponse>>> Handle(
        GetAllMemberPointHistoryPagedCommand request,
        CancellationToken cancellationToken)
    {
        Member? member;
        if (request.MemberId is null)
            member = await _memberRepository.GetByIdentityAsync(_memberContext.IdentityId, cancellationToken);
        else
            member = await _memberRepository.GetByIdAsync(request.MemberId, cancellationToken);
        if (member is null)
            return Result.Failure<PagedList<MemberPointHistoryResponse>>(MemberErrors.NotFound);
        var query = _memberPointHistoryRepository.GetEntitiesAsQueryable()
            .OrderByDescending(x => x.CreatedDate)
            .Where(x => x.MemberId.Equals(member.Id));
        if (request.PointType.HasValue)
        {
            query = query.Where(x => x.PointType == request.PointType.Value);
        }

        var result = await _memberPointHistoryRepository.GetAllPaged(request, query);
        var memberPointHistories = result.Data.Select(x => new MemberPointHistoryResponse()
        {
            Point = x.MemberPoint.Value,
            Title = x.Title.Value,
            CreatedAt = x.CreatedDate,
            PointType = x.PointType
        }).ToList();
        return Result.Success(
            new PagedList<MemberPointHistoryResponse>(memberPointHistories, result.TotalCount, result.CurrentPage,
                result.PageSize));
    }
}