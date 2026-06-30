using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.LeaveRequests.Cancel;
using Application.LeaveRequests.Create;
using Application.LeaveRequests.Get;
using Application.LeaveTypes.GetAll;
using Application.Employees.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Backend.Controllers;

[Authorize]
[Route("leave-request")]
public class LeaveRequestController : Controller
{
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;
    private readonly ISender _sender;

    public LeaveRequestController(ISender sender, IUserContext userContext, IRoleService roleService)
    {
        _sender = sender;
        _userContext = userContext;
        _roleService = roleService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] Guid? employeeId,
        [FromQuery] Guid? leaveTypeId,
        [FromQuery] int? status,
        CancellationToken cancellationToken)
    {
        string identityId = _userContext.IdentityId;

        // Kiểm tra quyền truy cập
        var checkViewPerm = await _roleService.checkRoleExist(identityId, "VIEW_LEAVE_REQUEST", cancellationToken);
        var checkApprovePerm = await _roleService.checkRoleExist(identityId, "APPROVE_LEAVE_REQUEST", cancellationToken);

        bool canView = checkViewPerm.Value;
        bool canApprove = checkApprovePerm.Value;

        if (!canView && !canApprove)
        {
            return Redirect("/NoPermission");
        }

        // Lấy danh sách đơn nghỉ phép
        var query = new GetLeaveRequestsQuery(employeeId, leaveTypeId, status);
        var result = await _sender.Send(query, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        // Load LeaveTypes active cho dropdown form xin nghỉ phép
        var leaveTypesResult = await _sender.Send(new GetAllLeaveTypesQuery(), cancellationToken);
        ViewBag.LeaveTypes = leaveTypesResult.Value ?? new();

        // Admin/HR có thể lọc theo employee
        if (canApprove)
        {
            var employeesResult = await _sender.Send(new GetAllEmployeesQuery(), cancellationToken);
            ViewBag.Employees = employeesResult.Value ?? new();
        }

        var checkCreatePerm = await _roleService.checkRoleExist(identityId, "CREATE_LEAVE_REQUEST", cancellationToken);
        ViewBag.CanCreate = checkCreatePerm.Value;
        ViewBag.CanApprove = canApprove;
        ViewBag.CurrentFilterEmployeeId = employeeId;
        ViewBag.CurrentFilterLeaveTypeId = leaveTypeId;
        ViewBag.CurrentFilterStatus = status;

        return View(result.Value);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] CreateLeaveRequestCommand command, CancellationToken cancellationToken)
    {
        var checkCreatePerm = await _roleService.checkRoleExist(_userContext.IdentityId, "CREATE_LEAVE_REQUEST", cancellationToken);
        if (!checkCreatePerm.Value)
        {
            return Redirect("/NoPermission");
        }

        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return Json(new { success = false, message = result.Error.Name });
        }

        return Json(new { success = true, message = "Đã gửi đơn xin nghỉ phép thành công." });
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel([FromForm] Guid id, CancellationToken cancellationToken)
    {
        var checkViewPerm = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_LEAVE_REQUEST", cancellationToken);
        if (!checkViewPerm.Value)
        {
            return Redirect("/NoPermission");
        }

        var command = new CancelLeaveRequestCommand(id);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return Json(new { success = false, message = result.Error.Name });
        }

        return Json(new { success = true, message = "Đã hủy đơn xin nghỉ phép thành công." });
    }
}
