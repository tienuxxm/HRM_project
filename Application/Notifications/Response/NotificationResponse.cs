namespace Application.Notifications.Response;

public class NotificationResponse
{
    public string Title { get; set; }
    public string Type { get; set; }
    public string? Content { get; set; }
    public Guid Id { get; set; }
    public Guid? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
}