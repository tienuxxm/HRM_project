using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.LeaveBalances.GetMyLeaveBalance;
using Application.LeaveRequests.GetApprovalAging;
using Application.LeaveRequests.GetDepartmentLeaveLoad;
using Application.LeaveRequests.GetLeaveStatusDistribution;
using Application.LeaveRequests.GetMonthlyLeaveTrend;
using Application.LeaveRequests.GetMyLeaveRequests;
using Application.LeaveRequests.GetPendingApprovals;
using Application.WorkCalendars.GetCalendarImpactAlerts;
using Application.WorkCalendars.GetUpcomingHolidays;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;

namespace Web.Backend.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ISender _sender;
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;

    public DashboardController(ISender sender, IUserContext userContext, IRoleService roleService)
    {
        _sender = sender;
        _userContext = userContext;
        _roleService = roleService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // Gate check: VIEW_DASHBOARD permission
        var checkDashboardPerm = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_DASHBOARD", cancellationToken);
        if (!checkDashboardPerm.Value)
        {
            return Redirect("/NoPermission");
        }

        var canViewDashboard = checkDashboardPerm.Value;
        var canViewLeaveRequest = (await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_LEAVE_REQUEST", cancellationToken)).Value;
        var canApproveLeaveRequest = (await _roleService.checkRoleExist(_userContext.IdentityId, "APPROVE_LEAVE_REQUEST", cancellationToken)).Value;
        var canViewLeaveBalance = (await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_LEAVE_BALANCE", cancellationToken)).Value;
        var canViewEmployee = (await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_EMPLOYEE", cancellationToken)).Value;
        var canViewDepartment = (await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_DEPARTMENT", cancellationToken)).Value;
        var canViewWorkCalendar = (await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_WORK_CALENDAR", cancellationToken)).Value;
        // Admin/HR global leave oversight: permission used by W4/W5 global queries
        var canViewAllApprovals = (await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken)).Value;

        var viewModel = new DashboardViewModel
        {
            CanViewDashboard = canViewDashboard,
            CanViewLeaveRequest = canViewLeaveRequest,
            CanApproveLeaveRequest = canApproveLeaveRequest,
            CanViewApprovalOversight = canViewAllApprovals,
            CanViewLeaveBalance = canViewLeaveBalance,
            CanViewEmployee = canViewEmployee,
            CanViewDepartment = canViewDepartment,
            CanViewWorkCalendar = canViewWorkCalendar
        };

        // --- Phase A2A/A2C/Phase 7: Dispatch MediatR queries for data-driven widgets ---

        // W1: My Leave Requests (personal scope)
        if (viewModel.ShowW1MyLeaveRequests)
        {
            var w1Result = await _sender.Send(new GetMyLeaveRequestsQuery(), cancellationToken);
            if (w1Result.IsSuccess)
            {
                viewModel.MyLeaveRequests = w1Result.Value;
            }
        }

        // W2: Leave Status Distribution (permission-aware scope)
        if (viewModel.ShowW2StatusDistribution)
        {
            var w2Result = await _sender.Send(new GetLeaveStatusDistributionQuery(), cancellationToken);
            if (w2Result.IsSuccess)
            {
                viewModel.StatusDistribution = w2Result.Value;
            }
        }

        // W3: Monthly Leave Trend (permission-aware scope)
        if (viewModel.ShowW3MonthlyTrend)
        {
            var w3Result = await _sender.Send(new GetMonthlyLeaveTrendQuery(), cancellationToken);
            if (w3Result.IsSuccess)
            {
                viewModel.MonthlyTrend = w3Result.Value;
            }
        }

        // W4: Approval Queue (Phase 7: Dynamic Approval Routing Source)
        if (viewModel.ShowW4ApprovalQueue)
        {
            var w4Result = await _sender.Send(new GetPendingApprovalsQuery(CanViewAllApprovals: canViewAllApprovals), cancellationToken);
            if (w4Result.IsSuccess)
            {
                viewModel.PendingApprovals = w4Result.Value;
            }
        }

        // W5: Approval Aging (Phase 7: Dynamic Approval Routing Source)
        if (viewModel.ShowW5ApprovalAging)
        {
            var w5Result = await _sender.Send(new GetApprovalAgingQuery(CanViewAllApprovals: canViewAllApprovals), cancellationToken);
            if (w5Result.IsSuccess)
            {
                viewModel.ApprovalAging = w5Result.Value;
            }
        }

        // W6: My Leave Balance (personal scope, Annual Leave only)
        if (viewModel.ShowW6MyLeaveBalance)
        {
            var w6Result = await _sender.Send(new GetMyLeaveBalanceQuery(), cancellationToken);
            if (w6Result.IsSuccess)
            {
                viewModel.MyLeaveBalance = w6Result.Value;
            }
        }

        // W8: Upcoming Holidays (global scope)
        if (viewModel.ShowW8UpcomingHolidays)
        {
            var w8Result = await _sender.Send(new GetUpcomingHolidaysQuery(), cancellationToken);
            if (w8Result.IsSuccess)
            {
                viewModel.UpcomingHolidays = w8Result.Value;
            }
        }

        // W9: Calendar Impact Alerts (Phase A2C)
        if (viewModel.ShowW9CalendarImpactAlerts)
        {
            var w9Result = await _sender.Send(new GetCalendarImpactAlertsQuery(), cancellationToken);
            if (w9Result.IsSuccess)
            {
                viewModel.CalendarImpactAlerts = w9Result.Value;
            }
        }

        // W10: Department Leave Load (Phase A2C)
        if (viewModel.ShowW10DepartmentLeaveLoad)
        {
            var w10Result = await _sender.Send(new GetDepartmentLeaveLoadQuery(), cancellationToken);
            if (w10Result.IsSuccess)
            {
                viewModel.DepartmentLeaveLoad = w10Result.Value;
            }
        }

        return View(viewModel);
    }
}
