using Domain.Abstractions;

namespace Application.Abstractions.FirebaseMessaging;

public interface IFirebaseMessaging
{
    Task<Result> SendNotification(string deviceToken, string message);
    Task<Result> SendMultipleNotification(List<FirebaseMessageRequest> messageRequests);
}

public class FirebaseMessageRequest
{
    public string DeviceToken { get; set; }
    public string Message { get; set; }
}