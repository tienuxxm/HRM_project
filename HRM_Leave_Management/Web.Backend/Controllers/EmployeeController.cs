using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.Departments.GetAll;
using Application.Employees.Create;
using Application.Employees.Delete;
using Application.Employees.GetAll;
using Application.Employees.Update;
using Application.Positions.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Backend.Controllers;

[Authorize]
[Route("employee")]
public class EmployeeController : Controller
{
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;
    private readonly ISender _sender;

    public EmployeeController(ISender sender, IUserContext userContext, IRoleService roleService)
    {
        _sender = sender;
        _userContext = userContext;
        _roleService = roleService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_EMPLOYEE", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        // Load departments for dropdown
        var deptQuery = new GetAllDepartmentsQuery();
        var deptResult = await _sender.Send(deptQuery, cancellationToken);
        ViewBag.Departments = deptResult.IsSuccess ? deptResult.Value : new();

        // Load positions for dropdown
        var posQuery = new GetAllPositionsQuery();
        var posResult = await _sender.Send(posQuery, cancellationToken);
        ViewBag.Positions = posResult.IsSuccess ? posResult.Value : new();

        var query = new GetAllEmployeesQuery();
        var result = await _sender.Send(query, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return View(result.Value);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] CreateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_EMPLOYEE", cancellationToken);
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
    public async Task<IActionResult> Update([FromForm] UpdateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_EMPLOYEE", cancellationToken);
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
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_EMPLOYEE", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }
        var command = new DeleteEmployeeCommand { Id = id };
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
            return RedirectToAction("Index");
        return BadRequest(result.Error);
    }
}
