using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.LeaveApproverAssignments.Create;
using Application.LeaveApproverAssignments.Update;
using Application.LeaveApproverAssignments.Delete;
using Application.LeaveApproverAssignments.GetAll;
using Application.Employees.GetAll;
using Application.Departments.GetAll;
using Application.Positions.GetAll;
using Domain.LeaveApproverAssignments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Backend.Controllers;

[Authorize]
[Route("leave-approver-assignment")]
public class LeaveApproverAssignmentController : Controller
{
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;
    private readonly ISender _sender;

    public LeaveApproverAssignmentController(ISender sender, IUserContext userContext, IRoleService roleService)
    {
        _sender = sender;
        _userContext = userContext;
        _roleService = roleService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        string identityId = _userContext.IdentityId;
        var checkViewPerm = await _roleService.checkRoleExist(identityId, "VIEW_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        if (!checkViewPerm.Value)
        {
            return Redirect("/NoPermission");
        }

        var checkUpdatePerm = await _roleService.checkRoleExist(identityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        ViewBag.CanUpdate = checkUpdatePerm.Value;

        // Load assignments
        var query = new GetAllLeaveApproverAssignmentsQuery();
        var assignmentsResult = await _sender.Send(query, cancellationToken);
        if (assignmentsResult.IsFailure)
        {
            return BadRequest(assignmentsResult.Error);
        }

        // Load metadata for dropdowns
        var employeesResult = await _sender.Send(new GetAllEmployeesQuery(), cancellationToken);
        ViewBag.Employees = employeesResult.Value ?? new();

        var departmentsResult = await _sender.Send(new GetAllDepartmentsQuery(), cancellationToken);
        ViewBag.Departments = departmentsResult.Value ?? new();

        var positionsResult = await _sender.Send(new GetAllPositionsQuery(), cancellationToken);
        ViewBag.Positions = positionsResult.Value ?? new();

        return View(assignmentsResult.Value);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] CreateLeaveApproverAssignmentCommand command, CancellationToken cancellationToken)
    {
        var checkUpdatePerm = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        if (!checkUpdatePerm.Value)
        {
            return Redirect("/NoPermission");
        }

        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            var msg = result.Error == LeaveApproverAssignmentErrors.DuplicateAssignment
                ? "Cấu hình phê duyệt này đã tồn tại (cùng người duyệt, phòng ban và chức vụ)."
                : result.Error.Name;
            return Json(new { success = false, message = msg });
        }

        return Json(new { success = true, message = "Thêm cấu hình phê duyệt thành công." });
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromForm] UpdateLeaveApproverAssignmentCommand command, CancellationToken cancellationToken)
    {
        var checkUpdatePerm = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        if (!checkUpdatePerm.Value)
        {
            return Redirect("/NoPermission");
        }

        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            var msg = result.Error == LeaveApproverAssignmentErrors.DuplicateAssignment
                ? "Cấu hình phê duyệt này đã tồn tại (cùng người duyệt, phòng ban và chức vụ)."
                : result.Error.Name;
            return Json(new { success = false, message = msg });
        }

        return Json(new { success = true, message = "Cập nhật cấu hình phê duyệt thành công." });
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var checkUpdatePerm = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        if (!checkUpdatePerm.Value)
        {
            return Redirect("/NoPermission");
        }

        var command = new DeleteLeaveApproverAssignmentCommand(id);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return Json(new { success = false, message = result.Error.Name });
        }

        return Json(new { success = true, message = "Xóa cấu hình phê duyệt thành công." });
    }
}
