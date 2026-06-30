using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.Departments.Create;
using Application.Departments.Delete;
using Application.Departments.GetAll;
using Application.Departments.Update;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Backend.Controllers;

[Authorize]
[Route("department")]
public class DepartmentController : Controller
{
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;
    private readonly ISender _sender;

    public DepartmentController(ISender sender, IUserContext userContext, IRoleService roleService)
    {
        _sender = sender;
        _userContext = userContext;
        _roleService = roleService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_DEPARTMENT", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }
        var query = new GetAllDepartmentsQuery();
        var result = await _sender.Send(query, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return View(result.Value);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] CreateDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_DEPARTMENT", cancellationToken);
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
    public async Task<IActionResult> Update([FromForm] UpdateDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_DEPARTMENT", cancellationToken);
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
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_DEPARTMENT", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }
        var command = new DeleteDepartmentCommand { Id = id };
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
            return RedirectToAction("Index");
        return BadRequest(result.Error);
    }
}
