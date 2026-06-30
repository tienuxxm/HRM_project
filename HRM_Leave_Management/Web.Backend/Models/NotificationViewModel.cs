using Application.Members.Responses;
using Application.Notifications.Response;
using Domain.Abstractions;

namespace Web.Backend.Models;

public class NotificationViewModel
{
    public PagedList<NotificationResponse> Notifications { get; set; }
    public List<MemberResponse> Members { get; set; }
}