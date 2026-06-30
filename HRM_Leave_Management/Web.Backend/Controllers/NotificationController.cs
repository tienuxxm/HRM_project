using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.Members.GetAll;
using Application.Notifications.CreateNotification;
using Application.Notifications.GetAll;
using Domain.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;

namespace Web.Backend.Controllers;

[Authorize]
[Route("notification")]
public class NotificationController : Controller
{
    private readonly IRoleService _roleService;
    private readonly ISender _sender;
    private readonly IUserContext _userContext;

    // GET
    public NotificationController(ISender sender, IUserContext userContext, IRoleService roleService)
    {
        _sender = sender;
        _userContext = userContext;
        _roleService = roleService;
    }

    public async Task<IActionResult> Index([FromQuery] PageQueryParam query, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_NOTIFICATION", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new GetAllNotificationPagedCommand
        {
            Page = query.Page,
            PageSize = query.PageSize,
            SearchTerm = query.SearchTerm,
            SortColumn = nameof(Notification.CreatedDate),
            SortOrder = "DESC"
        };

        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest();
        var getMemberCommand = new GetAllMemberCommand();
        var memberResult = await _sender.Send(getMemberCommand, cancellationToken);

        var notificationViewModel = new NotificationViewModel
        {
            Notifications = result.Value,
            Members = memberResult.Value
        };
        return View(notificationViewModel);
    }

    [HttpPost("create-notification")]
    public async Task<IActionResult> CreateNotification(string title, string content, List<Guid> memberIds,
        CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_NOTIFICATION", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");
        var command = new CreateNotificationCommand(title, "SYSTEM_NOTIFICATION", memberIds, content);

        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok();
    }
}