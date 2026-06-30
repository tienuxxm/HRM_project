using Application.Abstractions.FirebaseMessaging;
using Application.Abstractions.Messaging;
using Domain.Abstractions;

namespace Application.SendNotification;

public class SendNotificationCommandHandler : ICommandHandler<SendNotificationCommand>
{
    private readonly IFirebaseMessaging _firebaseMessaging;

    public SendNotificationCommandHandler(IFirebaseMessaging firebaseMessaging)
    {
        _firebaseMessaging = firebaseMessaging;
    }

    public async Task<Result> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await Task.CompletedTask;
            await _firebaseMessaging.SendNotification(request.Token, request.Message);
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(Error.None);
        }
    }
}