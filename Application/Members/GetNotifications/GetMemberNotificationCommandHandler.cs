using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.MemberNotifications;
using Domain.Members;

namespace Application.Members.GetNotifications;

public class
    GetMemberNotificationCommandHandler : ICommandHandler<GetMemberNotificationCommand,
        PagedList<MemberNotificationResponse>>
{
    private readonly IMemberContext _memberContext;
    private readonly IMemberRepository _memberRepository;
    private readonly IMemberNotificationRepository _memberNotificationRepository;

    public GetMemberNotificationCommandHandler(IMemberContext memberContext,
        IMemberNotificationRepository memberNotificationRepository, IMemberRepository memberRepository)
    {
        _memberContext = memberContext;
        _memberNotificationRepository = memberNotificationRepository;
        _memberRepository = memberRepository;
    }

    public async Task<Result<PagedList<MemberNotificationResponse>>> Handle(GetMemberNotificationCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_memberContext.IdentityId))
            return Result.Failure<PagedList<MemberNotificationResponse>>(MemberErrors.InvalidCredentials);
        var member =
            await _memberRepository.GetByIdentityAsync(_memberContext.IdentityId, cancellationToken);
        if (member is null)
            return Result.Failure<PagedList<MemberNotificationResponse>>(MemberErrors.NotFound);

        var query = _memberNotificationRepository.GetEntitiesAsQueryable()
            .Where(x => x.MemberId.Equals(member.Id));
        var result = await _memberNotificationRepository.GetAllPaged(request, query);
        var resultResponse = result.Data.Select(x => new MemberNotificationResponse()
        {
            Id = x.Id.Value,
            Content = x.Content.Value,
            Title = x.Title.Value,
            Type = x.NotificationType.Value,
            CreatedAt = x.CreatedAt,
            IsRead = x.IsReaded,
            ReferenceId = x.ReferenceId?.Value.ToString()
        }).ToList();
        return Result.Success(new PagedList<MemberNotificationResponse>(resultResponse, result.TotalCount,
            result.CurrentPage,
            result.PageSize));
    }
}