using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.RestaurantArea.Create;
using Application.RestaurantArea.Delete;
using Application.RestaurantArea.GetAllPaged;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;

namespace Web.Backend.Controllers;

[Authorize]
[Route("area")]
public class AreaController : Controller
{    
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;
    private readonly ISender _sender;

    // GET
    public AreaController(ISender sender, IUserContext userContext, IRoleService roleService)
    {
        _sender = sender;
        _userContext = userContext;
        _roleService = roleService;
    }

    public async Task<IActionResult> Index([FromQuery] PageQueryParam query, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_AREA", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return  Redirect("/NoPermission");
        }
        var command = new GetAllAreaPagedCommand()
        {
            Page = query.Page,
            PageSize = query.PageSize,
            SearchTerm = query.SearchTerm,
            SortColumn = query.SortColumn,
            SortOrder = query.SortOrder
        };
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return View(result.Value);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] CreateAreaViewModel model, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_AREA", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return  Redirect("/NoPermission");
        }
        var command = new CreateAreaCommand(model.AreaName);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_AREA", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return  Redirect("/NoPermission");
        }
        var command = new DeleteAreaCommand(id);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
            return RedirectToAction("Index");
        return NoContent();
    }
}