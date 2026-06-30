using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.MemberNotifications;
using Domain.Members;
using Domain.Notifications;
using Domain.Shared;
using Content = Domain.Notifications.Content;
using ReferenceId = Domain.MemberNotifications.ReferenceId;

namespace Application.Notifications.CreateNotification;

public class CreateNotificationCommandHandler : ICommandHandler<CreateNotificationCommand>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMemberNotificationRepository _memberNotificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemberRepository _memberRepository;

    public CreateNotificationCommandHandler(IDateTimeProvider dateTimeProvider, IUnitOfWork unitOfWork,
        IMemberRepository memberRepository, IMemberNotificationRepository memberNotificationRepository)
    {
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
        _memberRepository = memberRepository;
        _memberNotificationRepository = memberNotificationRepository;
    }

    public async Task<Result> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            List<Guid> memberIds = new() { };
            if (request.memberIds.Count > 0)
            {
                memberIds.AddRange(request.memberIds);
            }
            else
            {
                var members = (await _memberRepository.GetAll(cancellationToken))?.Select(x => x.Id.Value).ToList();
                if (members is not null)
                    memberIds.AddRange(members);
            }

            var notifications = memberIds.Select(id => MemberNotification.Create(new MemberId(id),
                new Title(request.Title),
                new Domain.MemberNotifications.Content(request.Content ?? ""),
                new MemberNotificationType(NotificationTypes.System), new ReferenceId(id), DateTime.UtcNow)).ToList();
            _memberNotificationRepository.AddRange(notifications);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(new Error("CreateNotification.Fail", "Fail to create notification"));
        }
    }
}