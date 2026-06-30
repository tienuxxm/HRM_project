using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.MemberNotifications;

namespace Application.MemberNotifications;


public class DeleteMemberNotificationCommandHandler: ICommandHandler<DeleteMemberNotificationCommand>
{
    private readonly IMemberNotificationRepository _memberNotificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMemberNotificationCommandHandler(IMemberNotificationRepository memberNotificationRepository, IUnitOfWork unitOfWork)
    {
        _memberNotificationRepository = memberNotificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteMemberNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await _memberNotificationRepository.GetByIdAsync(request.Id, cancellationToken);
        if(notification is null)
            return Result.Failure(new Error("MemberNotification.NotFound", "Member Notification Not Found"));
        _memberNotificationRepository.Remove(notification);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}