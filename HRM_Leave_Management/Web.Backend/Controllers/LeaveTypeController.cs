using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.LeaveTypes.Create;
using Application.LeaveTypes.Delete;
using Application.LeaveTypes.GetAll;
using Application.LeaveTypes.GetPaged;
using Application.LeaveTypes.Update;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Backend.Controllers;

[Authorize]
[Route("leave-type")]
public class LeaveTypeController : Controller
{
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;
    private readonly ISender _sender;

    public LeaveTypeController(ISender sender, IUserContext userContext, IRoleService roleService)
    {
        _sender = sender;
        _userContext = userContext;
        _roleService = roleService;
    }

    public async Task<IActionResult> Index(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5,
        CancellationToken cancellationToken = default)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_LEAVE_TYPE", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }
        var query = new GetPagedLeaveTypesQuery(page, pageSize);
        var result = await _sender.Send(query, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);

        ViewBag.CurrentPage = result.Value.CurrentPage;
        ViewBag.TotalPages = result.Value.TotalPages;
        ViewBag.PageSize = result.Value.PageSize;
        ViewBag.TotalCount = result.Value.TotalCount;

        return View(result.Value.Data);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] CreateLeaveTypeCommand command,
        CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_TYPE", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromForm] UpdateLeaveTypeCommand command,
        CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_TYPE", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_TYPE", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }
        var command = new DeleteLeaveTypeCommand { Id = id };
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
            return RedirectToAction("Index");
        return BadRequest(result.Error);
    }
}
