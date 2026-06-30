namespace Application.Members.GetNotifications;

public class MemberNotificationResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ReferenceId { get; set; }
    public bool IsRead { get; set; }
}