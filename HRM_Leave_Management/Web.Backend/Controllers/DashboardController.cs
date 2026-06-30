using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.Bookings.BookingReport;
using Application.Orders.GetRevenue;
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

    public async Task<IActionResult> Index([FromQuery] int? revenueRangeType, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_DASHBOARD", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        var dashboardViewModel = new DashboardViewModel();
        var command = new GetBookingReportCommand();
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
            dashboardViewModel.BookingReportResponse = result.Value;

        var revenueCommand = new GetRevenueCommand(revenueRangeType.HasValue
            ? (RevenueDataRangeType)revenueRangeType.Value
            : RevenueDataRangeType.Week);
        var revenueResult = await _sender.Send(revenueCommand, cancellationToken);
        if (revenueResult.IsSuccess)
            dashboardViewModel.RevenueResponse = revenueResult.Value;

        dashboardViewModel.RevenueRange =
            revenueRangeType ?? (int)RevenueDataRangeType.Week;
        return View(dashboardViewModel);
    }

    [HttpGet("get-revenue/{rangeType:int}")]
    public async Task<IActionResult> GetRevenue(int rangeType, CancellationToken cancellationToken)
    {
        var command = new GetRevenueCommand((RevenueDataRangeType)rangeType);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok(result.Value);
    }
}