using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.WorkCalendars.GetWorkCalendarDays;
using Application.WorkCalendars.GetImportBatchDetails;
using Application.WorkCalendars.GetImportBatchSummary;
using Application.WorkCalendars.UploadImportBatch;
using Application.WorkCalendars.ConfirmImportBatch;
using Application.WorkCalendars.PreviewManualCalendarChange;
using Application.WorkCalendars.SaveManualCalendarDay;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Web.Backend.Controllers;

[Authorize]
[Route("work-calendar")]
public class WorkCalendarController : Controller
{
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;
    private readonly ISender _sender;

    public WorkCalendarController(ISender sender, IUserContext userContext, IRoleService roleService)
    {
        _sender = sender;
        _userContext = userContext;
        _roleService = roleService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] int? year, [FromQuery] int? month, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_WORK_CALENDAR", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        var query = new GetWorkCalendarDaysQuery(year, month);
        var result = await _sender.Send(query, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        var updatePerm = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_WORK_CALENDAR", cancellationToken);
        ViewBag.CanUpdate = updatePerm.Value;
        ViewBag.CurrentYear = year ?? DateTime.Today.Year;
        ViewBag.CurrentMonth = month;

        return View(result.Value);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_WORK_CALENDAR", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "No file uploaded or file is empty." });
        }

        var command = new UploadCalendarImportBatchCommand(file.OpenReadStream(), file.FileName);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return Json(new { success = false, message = result.Error.Name });
        }

        return Json(new { success = true, batchId = result.Value });
    }

    [HttpGet("preview/{batchId:guid}")]
    public async Task<IActionResult> Preview([FromRoute] Guid batchId, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_WORK_CALENDAR", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        var query = new GetCalendarImportBatchDetailsQuery(batchId);
        var result = await _sender.Send(query, cancellationToken);
        if (result.IsFailure)
        {
            return RedirectToAction(nameof(Index));
        }

        var updatePerm = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_WORK_CALENDAR", cancellationToken);
        ViewBag.CanUpdate = updatePerm.Value;

        return View(result.Value);
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromForm] Guid batchId, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_WORK_CALENDAR", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        var command = new ConfirmCalendarImportBatchCommand(batchId);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return Json(new { success = false, message = result.Error.Name });
        }

        return Json(new { success = true });
    }

    [HttpGet("summary/{batchId:guid}")]
    public async Task<IActionResult> Summary([FromRoute] Guid batchId, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_WORK_CALENDAR", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        var query = new GetCalendarImportBatchSummaryQuery(batchId);
        var result = await _sender.Send(query, cancellationToken);
        if (result.IsFailure)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(result.Value);
    }

    [HttpGet("download-template")]
    public async Task<IActionResult> DownloadTemplate(CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_WORK_CALENDAR", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        using (var package = new OfficeOpenXml.ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("WorkCalendarTemplate");
            worksheet.Cells[1, 1].Value = "Date";
            worksheet.Cells[1, 2].Value = "DayType";
            worksheet.Cells[1, 3].Value = "WorkShift";
            worksheet.Cells[1, 4].Value = "Description";

            worksheet.Cells[2, 1].Value = "2026-09-02";
            worksheet.Cells[2, 2].Value = "PublicHoliday";
            worksheet.Cells[2, 3].Value = "None";
            worksheet.Cells[2, 4].Value = "National Day";

            worksheet.Cells[3, 1].Value = "2026-09-05";
            worksheet.Cells[3, 2].Value = "StandardWorkingDayOverride";
            worksheet.Cells[3, 3].Value = "FullDay";
            worksheet.Cells[3, 4].Value = "Make up day";

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var fileBytes = package.GetAsByteArray();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "WorkCalendar_Template.xlsx");
        }
    }

    [HttpPost("preview-manual")]
    public async Task<IActionResult> PreviewManual([FromBody] PreviewManualRequest model, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_WORK_CALENDAR", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Json(new { success = false, message = "NoPermission" });
        }

        if (model == null)
        {
            return Json(new { success = false, message = "Invalid request data." });
        }

        var query = new PreviewManualCalendarChangeQuery(model.Date, model.DayType, model.WorkShift, model.IsActive);
        var result = await _sender.Send(query, cancellationToken);
        if (result.IsFailure)
        {
            return Json(new { success = false, message = result.Error.Name });
        }

        return Json(new { success = true, affectedRequests = result.Value });
    }

    [HttpPost("save-manual")]
    public async Task<IActionResult> SaveManual([FromBody] SaveManualRequest model, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_WORK_CALENDAR", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Json(new { success = false, message = "NoPermission" });
        }

        if (model == null)
        {
            return Json(new { success = false, message = "Invalid request data." });
        }

        var command = new SaveManualCalendarDayCommand(model.Date, model.DayType, model.WorkShift, model.Description, model.IsActive);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return Json(new { success = false, message = result.Error.Name });
        }

        return Json(new { success = true });
    }
}

public record PreviewManualRequest(DateOnly Date, string DayType, string WorkShift, bool IsActive);
public record SaveManualRequest(DateOnly Date, string DayType, string WorkShift, string? Description, bool IsActive);

