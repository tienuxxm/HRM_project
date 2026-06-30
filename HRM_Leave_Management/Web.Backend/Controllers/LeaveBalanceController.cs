using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.Employees.GetAll;
using Application.LeaveBalances.Create;
using Application.LeaveBalances.Delete;
using Application.LeaveBalances.Get;
using Application.LeaveBalances.Update;
using Application.LeaveTypes.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Backend.Controllers;

[Authorize]
[Route("leave-balance")]
public class LeaveBalanceController : Controller
{
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;
    private readonly ISender _sender;

    public LeaveBalanceController(ISender sender, IUserContext userContext, IRoleService roleService)
    {
        _sender = sender;
        _userContext = userContext;
        _roleService = roleService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] Guid? employeeId,
        [FromQuery] Guid? leaveTypeId,
        [FromQuery] int? year,
        CancellationToken cancellationToken)
    {
        string identityId = _userContext.IdentityId;

        // 1. Kiểm tra quyền xem hoặc cập nhật số dư phép
        var checkViewPerm = await _roleService.checkRoleExist(identityId, "VIEW_LEAVE_BALANCE", cancellationToken);
        var checkUpdatePerm = await _roleService.checkRoleExist(identityId, "UPDATE_LEAVE_BALANCE", cancellationToken);

        bool canView = checkViewPerm.Value;
        bool canUpdate = checkUpdatePerm.Value;

        if (!canView && !canUpdate)
        {
            return Redirect("/NoPermission");
        }

        // 2. Lấy danh sách số dư phép (Query Handler tự quyết định logic filter dựa trên quyền)
        var query = new GetLeaveBalancesQuery(employeeId, leaveTypeId, year);
        var result = await _sender.Send(query, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        // 3. Nếu là Admin/HR (có quyền UPDATE_LEAVE_BALANCE), load thêm danh sách employee và leave type để render dropdown
        if (canUpdate)
        {
            var employeesResult = await _sender.Send(new GetAllEmployeesQuery(), cancellationToken);
            ViewBag.Employees = employeesResult.Value ?? new();

            var leaveTypesResult = await _sender.Send(new GetAllLeaveTypesQuery(), cancellationToken);
            ViewBag.LeaveTypes = leaveTypesResult.Value ?? new();
        }

        ViewBag.CanUpdate = canUpdate;
        ViewBag.CurrentFilterEmployeeId = employeeId;
        ViewBag.CurrentFilterLeaveTypeId = leaveTypeId;
        ViewBag.CurrentFilterYear = year;

        return View(result.Value);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] CreateLeaveBalanceCommand command, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_BALANCE", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromForm] UpdateLeaveBalanceCommand command, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_BALANCE", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_BALANCE", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        var command = new DeleteLeaveBalanceCommand(id);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
        {
            return RedirectToAction("Index");
        }

        return BadRequest(result.Error);
    }

    [HttpGet("get-by-id/{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetLeaveBalanceByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);
        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Json(result.Value);
    }
}
