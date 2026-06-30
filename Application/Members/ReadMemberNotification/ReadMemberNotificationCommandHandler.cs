using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.MemberNotifications;

namespace Application.Members.ReadMemberNotification;

public class ReadMemberNotificationCommandHandler : ICommandHandler<ReadMemberNotificationCommand>
{
    private readonly IMemberNotificationRepository _memberNotificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReadMemberNotificationCommandHandler(IMemberNotificationRepository memberNotificationRepository,
        IUnitOfWork unitOfWork)
    {
        _memberNotificationRepository = memberNotificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ReadMemberNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification =
            await _memberNotificationRepository.GetByIdAsync(new MemberNotificationId(request.NotificationId),
                cancellationToken);
        if (notification is null)
            return Result.Failure(Error.None);
        notification.Read();
        _memberNotificationRepository.Update(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}