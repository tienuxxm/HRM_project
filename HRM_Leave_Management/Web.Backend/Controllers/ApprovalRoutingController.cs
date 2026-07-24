using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.ApprovalRouting.Commands.AddApprovalRouteLevel;
using Application.ApprovalRouting.Commands.AddApprovalRouteRule;
using Application.ApprovalRouting.Commands.AssignApprovalRouteLevel;
using Application.ApprovalRouting.Commands.CreateApprovalRoutePolicy;
using Application.ApprovalRouting.Commands.UnassignApprovalLevel;
using Application.ApprovalRouting.Queries.GetApprovalRoutePolicies;
using Application.ApprovalRouting.Queries.GetApprovalRoutePolicyDetail;
using Application.ApprovalRouting.Queries.GetEligibleManualApprovers;
using Application.ApprovalRouting.Queries.GetLevelAssignmentUnassignImpact;
using Application.ApprovalRouting.Queries.GetLevelSlotAssignments;
using Application.Departments.GetAll;
using Application.Employees.GetAll;
using Application.Positions.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models.ApprovalRouting;

namespace Web.Backend.Controllers;

[Authorize]
[Route("approval-routing")]
public class ApprovalRoutingController : Controller
{
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;
    private readonly ISender _sender;

    public ApprovalRoutingController(
        ISender sender,
        IUserContext userContext,
        IRoleService roleService)
    {
        _sender = sender;
        _userContext = userContext;
        _roleService = roleService;
    }

    [HttpGet("policies")]
    public async Task<IActionResult> Policies(
        [FromQuery] string? department,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var viewPerm = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        if (!viewPerm.Value)
        {
            return Redirect("/NoPermission");
        }

        var updatePerm = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);

        var policiesResult = await _sender.Send(new GetApprovalRoutePoliciesQuery(department, search), cancellationToken);
        var departmentsResult = await _sender.Send(new GetAllDepartmentsQuery(), cancellationToken);

        var model = new PolicyListViewModel
        {
            Policies = policiesResult.Value ?? new(),
            Departments = departmentsResult.Value ?? new(),
            DepartmentFilter = department,
            SearchTerm = search,
            CanUpdate = updatePerm.Value
        };

        return View("~/Views/ApprovalRouting/Policies.cshtml", model);
    }

    [HttpGet("policies/create")]
    public async Task<IActionResult> CreatePolicy(CancellationToken cancellationToken)
    {
        var updatePerm = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        if (!updatePerm.Value)
        {
            return Redirect("/NoPermission");
        }

        var departmentsResult = await _sender.Send(new GetAllDepartmentsQuery(), cancellationToken);
        ViewBag.Departments = departmentsResult.Value ?? new();

        return View("~/Views/ApprovalRouting/CreatePolicy.cshtml");
    }

    [HttpPost("policies/create")]
    public async Task<IActionResult> CreatePolicyPost(
        [FromForm] CreateApprovalRoutePolicyCommand command,
        CancellationToken cancellationToken)
    {
        var updatePerm = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        if (!updatePerm.Value)
        {
            return Redirect("/NoPermission");
        }

        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Error.Name;
            var departmentsResult = await _sender.Send(new GetAllDepartmentsQuery(), cancellationToken);
            ViewBag.Departments = departmentsResult.Value ?? new();
            return View("~/Views/ApprovalRouting/CreatePolicy.cshtml");
        }

        return RedirectToAction(nameof(PolicyDetail), new { id = result.Value });
    }

    [HttpGet("policies/detail/{id:guid}")]
    public async Task<IActionResult> PolicyDetail(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var viewPerm = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        if (!viewPerm.Value)
        {
            return Redirect("/NoPermission");
        }

        var updatePerm = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);

        var detailResult = await _sender.Send(new GetApprovalRoutePolicyDetailQuery(id), cancellationToken);
        if (detailResult.IsFailure)
        {
            return RedirectToAction(nameof(Policies));
        }

        var positionsResult = await _sender.Send(new GetAllPositionsQuery(), cancellationToken);
        var employeesResult = await _sender.Send(new GetAllEmployeesQuery(), cancellationToken);

        var model = new PolicyDetailViewModel
        {
            Detail = detailResult.Value,
            Positions = positionsResult.Value ?? new(),
            Employees = employeesResult.Value ?? new(),
            CanUpdate = updatePerm.Value
        };

        return View("~/Views/ApprovalRouting/PolicyDetail.cshtml", model);
    }

    [HttpPost("levels/add")]
    public async Task<IActionResult> AddLevel(
        [FromForm] AddApprovalRouteLevelCommand command,
        CancellationToken cancellationToken)
    {
        var updatePerm = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        if (!updatePerm.Value)
        {
            return Json(new { success = false, message = "NoPermission" });
        }

        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return Json(new { success = false, message = result.Error.Name });
        }

        return Json(new { success = true, message = "Level added successfully.", levelId = result.Value });
    }

    [HttpPost("rules/add")]
    public async Task<IActionResult> AddRule(
        [FromForm] AddApprovalRouteRuleCommand command,
        CancellationToken cancellationToken)
    {
        var updatePerm = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        if (!updatePerm.Value)
        {
            return Json(new { success = false, message = "NoPermission" });
        }

        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return Json(new { success = false, message = result.Error.Name });
        }

        return Json(new { success = true, message = "Rule added successfully.", ruleId = result.Value });
    }

    [HttpPost("levels/assign")]
    public async Task<IActionResult> AssignLevel(
        [FromForm] AssignApprovalRouteLevelCommand command,
        CancellationToken cancellationToken)
    {
        var updatePerm = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        if (!updatePerm.Value)
        {
            return Json(new { success = false, message = "NoPermission" });
        }

        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return Json(new { success = false, message = result.Error.Name });
        }

        return Json(new { success = true, message = "Level slot assigned successfully.", assignmentId = result.Value });
    }

    [HttpGet("levels/assignments")]
    public async Task<IActionResult> LevelAssignments(
        [FromQuery] Guid? policyId,
        CancellationToken cancellationToken)
    {
        var viewPerm = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        if (!viewPerm.Value)
        {
            return Redirect("/NoPermission");
        }

        var updatePerm = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);

        if (!policyId.HasValue || policyId.Value == Guid.Empty)
        {
            var allPolicies = await _sender.Send(new GetApprovalRoutePoliciesQuery(), cancellationToken);
            var firstPolicy = allPolicies.Value?.FirstOrDefault(p => p.IsActive) ?? allPolicies.Value?.FirstOrDefault();
            if (firstPolicy == null)
            {
                return RedirectToAction(nameof(Policies));
            }
            policyId = firstPolicy.PolicyId;
        }

        var result = await _sender.Send(new GetLevelSlotAssignmentsQuery(policyId.Value), cancellationToken);
        if (result.IsFailure)
        {
            return RedirectToAction(nameof(Policies));
        }

        var model = new LevelAssignmentViewModel
        {
            Data = result.Value,
            CanUpdate = updatePerm.Value
        };

        var employeesResult = await _sender.Send(new GetAllEmployeesQuery(), cancellationToken);
        ViewBag.Employees = employeesResult.Value ?? new();

        return View("~/Views/ApprovalRouting/LevelAssignments.cshtml", model);
    }

    [HttpPost("impact-preview")]
    public async Task<IActionResult> ImpactPreview(
        [FromForm] Guid levelAssignmentId,
        CancellationToken cancellationToken)
    {
        var updatePerm = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        if (!updatePerm.Value)
        {
            return Json(new { success = false, message = "NoPermission" });
        }

        var impactResult = await _sender.Send(new GetLevelAssignmentUnassignImpactQuery(levelAssignmentId), cancellationToken);
        if (impactResult.IsFailure)
        {
            return Json(new { success = false, message = impactResult.Error.Name });
        }

        var impactData = impactResult.Value;
        var eligibleApproversResult = await _sender.Send(new GetEligibleManualApproversQuery(impactData.TargetEmployeeId, levelAssignmentId), cancellationToken);
        if (eligibleApproversResult.IsFailure)
        {
            return Json(new { success = false, message = eligibleApproversResult.Error.Name });
        }

        var model = new ImpactPreviewModalViewModel
        {
            TargetLevelAssignmentId = levelAssignmentId,
            TargetEmployeeId = impactData.TargetEmployeeId,
            TargetSlotName = impactData.LevelName,
            AssignedEmployeeName = impactData.AssignedEmployeeName,
            ImpactData = impactData,
            AvailableApprovers = eligibleApproversResult.Value
        };

        return PartialView("~/Views/ApprovalRouting/_ImpactPreviewModal.cshtml", model);
    }

    [HttpPost("execute-reassignment")]
    public async Task<IActionResult> ExecuteReassignment(
        [FromForm] Guid levelAssignmentId,
        [FromForm] Guid? newApproverEmployeeId,
        [FromForm] bool autoRerouteUsingResolver,
        [FromForm] string? reason,
        CancellationToken cancellationToken)
    {
        var updatePerm = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        if (!updatePerm.Value)
        {
            return Json(new { success = false, message = "NoPermission" });
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var note = reason ?? "Operator level slot unassignment via UI console";

        var command = new UnassignApprovalLevelCommand(
            levelAssignmentId,
            today,
            newApproverEmployeeId,
            autoRerouteUsingResolver,
            note);

        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return Json(new { success = false, message = result.Error.Name });
        }

        return Json(new { success = true, message = "Level slot unassigned and affected pending leave requests reassigned successfully." });
    }
}
