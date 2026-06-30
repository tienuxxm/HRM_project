using Domain.Abstractions;
using Domain.MemberNotifications;
using Domain.Members;
using Domain.Members.Events;
using Domain.Notifications;
using Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Content = Domain.MemberNotifications.Content;
using ReferenceId = Domain.MemberNotifications.ReferenceId;

namespace Application.Members.AssignedMembershipClass;

public class AssignedMembershipClassEventHandler : INotificationHandler<AssignedMembershipClassDomainEvent>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IMemberNotificationRepository _memberNotificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignedMembershipClassEventHandler(IMemberRepository memberRepository,
        IMemberNotificationRepository memberNotificationRepository, IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _memberNotificationRepository = memberNotificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AssignedMembershipClassDomainEvent notification, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetEntitiesAsQueryable()
            .Include(x => x.MembershipClass)
            .FirstOrDefaultAsync(cancellationToken);

        /*if (member?.MembershipClass != null)
        {
            var memberNotification = MemberNotification.Create(notification.MemberId,
                new Title("Chúc mừng bạn vừa được thăng Rank " + member.MembershipClass.ClassName.Value),
                new Content("Chúc mừng bạn vừa được thăng Rank " + member.MembershipClass.ClassName.Value),
                new MemberNotificationType(NotificationTypes.Member), new ReferenceId(notification.MemberId.Value),
                DateTime.UtcNow);

            _memberNotificationRepository.Add(memberNotification);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }*/
    }
}