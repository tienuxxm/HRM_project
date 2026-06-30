using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.MemberNotifications;
using Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace Application.MemberNotifications.IsContainUnreadNotification;

public class IsContainUnreadNotificationCommandHandler : ICommandHandler<IsContainUnreadNotificationCommand, bool>
{
    private readonly IMemberContext _memberContext;
    private readonly IMemberNotificationRepository _memberNotificationRepository;
    private readonly IMemberRepository _memberRepository;

    public IsContainUnreadNotificationCommandHandler(IMemberContext memberContext,
        IMemberNotificationRepository memberNotificationRepository, IMemberRepository memberRepository)
    {
        _memberContext = memberContext;
        _memberNotificationRepository = memberNotificationRepository;
        _memberRepository = memberRepository;
    }

    public async Task<Result<bool>> Handle(IsContainUnreadNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdentityAsync(_memberContext.IdentityId, cancellationToken);
        if (member is null)
            return Result.Failure<bool>(MemberErrors.NotFound);
        var result = await _memberNotificationRepository.GetEntitiesAsQueryable()
            .AnyAsync(x => !x.IsReaded, cancellationToken);
        return Result.Success(result);
    }
}