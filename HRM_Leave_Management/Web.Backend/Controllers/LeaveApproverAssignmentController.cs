using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.LeaveApproverAssignments.GetAll;
using Application.Employees.GetAll;
using Application.Departments.GetAll;
using Application.Positions.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Backend.Controllers;

/// <summary>
/// Legacy Leave Approver Assignment Controller (Phase 8 Deprecated / Read-Only):
///   - Retained strictly for Read-Only Audit history.
///   - All Create, Update, Delete actions are permanently disabled.
///   - Source of Truth: Dynamic Approval Routing (/approval-routing/policies).
/// </summary>
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

        // Phase 8: Legacy module is strictly READ-ONLY. Disable mutation flag.
        ViewBag.CanUpdate = false;
        ViewBag.IsLegacyReadOnly = true;

        // Load legacy assignments for audit history
        var query = new GetAllLeaveApproverAssignmentsQuery();
        var assignmentsResult = await _sender.Send(query, cancellationToken);
        if (assignmentsResult.IsFailure)
        {
            return BadRequest(assignmentsResult.Error);
        }

        // Load metadata for dropdowns (read-only view)
        var employeesResult = await _sender.Send(new GetAllEmployeesQuery(), cancellationToken);
        ViewBag.Employees = employeesResult.Value ?? new();

        var departmentsResult = await _sender.Send(new GetAllDepartmentsQuery(), cancellationToken);
        ViewBag.Departments = departmentsResult.Value ?? new();

        var positionsResult = await _sender.Send(new GetAllPositionsQuery(), cancellationToken);
        ViewBag.Positions = positionsResult.Value ?? new();

        return View(assignmentsResult.Value);
    }

    [HttpPost("create")]
    public IActionResult Create()
    {
        return Json(new { success = false, message = "This feature has been retired (LEGACY READ-ONLY). Please manage dynamic superior routing under Dynamic Approval Routing Policies at /approval-routing/policies." });
    }

    [HttpPost("update")]
    public IActionResult Update()
    {
        return Json(new { success = false, message = "This feature has been retired (LEGACY READ-ONLY). Please manage dynamic superior routing under Dynamic Approval Routing Policies at /approval-routing/policies." });
    }

    [HttpPost("delete")]
    public IActionResult Delete()
    {
        return Json(new { success = false, message = "This feature has been retired (LEGACY READ-ONLY). Please manage dynamic superior routing under Dynamic Approval Routing Policies at /approval-routing/policies." });
    }
}
