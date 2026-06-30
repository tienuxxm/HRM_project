using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.InvoiceHistories;
using Application.MemberPointHistories.GetAllPaged;
using Application.Members.Create;
using Application.Members.Export;
using Application.Members.GetAllPaged;
using Application.Members.GetOne;
using Application.Members.Import;
using Application.Members.Search;
using Application.Members.UpdateBySystem;
using Application.MemberVoucher.GetAllPaged;
using Domain.Abstractions;
using Domain.Extension;
using Domain.Members;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;

namespace Web.Backend.Controllers;

[Route("Member")]
[Authorize]
public class MemberController : Controller
{
    private readonly IRoleService _roleService;
    private readonly ISender _sender;
    private readonly List<string> _tabs = new() { "vouchers", "point-histories", "invoice-histories" };
    private readonly IUserContext _userContext;

    public MemberController(ISender sender, IUserContext userContext, IRoleService roleService)
    {
        _sender = sender;
        _userContext = userContext;
        _roleService = roleService;
    }

    public async Task<IActionResult> LoadData(CancellationToken cancellationToken)
    {
        // Retrieve request parameters
        Request.Form.TryGetValue("length", out var length);
        Request.Form.TryGetValue("draw", out var draw);
        Request.Form.TryGetValue("start", out var start);
        Request.Form.TryGetValue("order[0][column]", out var column);
        Request.Form.TryGetValue("order[0][dir]", out var order);
        Request.Form.TryGetValue("search[value]", out var search);
        var lengthValue = int.Parse(length.ToString());
        var startValue = int.Parse(start.ToString());

        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_CUSTOMER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new GetAllMemberPagedCommand
        {
            Page = (startValue + 10) / 10,
            PageSize = lengthValue > 0 ? lengthValue : 10
        };

        var columnOrder = column.ToString() switch
        {
            "1" => nameof(Member.MemberCode),
            "2" => nameof(Member.FullName),
            "3" => nameof(Member.Email),
            "4" => nameof(Member.PhoneNumber),
            "5" => nameof(Member.Address),
            _ => null
        };

        if (!string.IsNullOrEmpty(search)) command.SearchTerm = search.ToString().Trim();

        if (!string.IsNullOrEmpty(columnOrder))
        {
            command.SortColumn = columnOrder;
            command.SortOrder = order.ToString().ToUpper();
        }

        var result = await _sender.Send(command, cancellationToken);


        var jsonData = new
        {
            draw = Convert.ToInt32(draw),
            recordsFiltered = result.Value.TotalCount,
            recordsTotal = result.Value.Data.Count,
            data = result.Value.Data,
            pages = Math.Round((double)result.Value.TotalCount / lengthValue, MidpointRounding.AwayFromZero)
        };

        return Ok(jsonData);
    }

    // GET
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] PageQueryParam queryParam, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_CUSTOMER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new GetAllMemberPagedCommand
        {
            Page = queryParam.Page,
            PageSize = queryParam.PageSize,
            SearchTerm = queryParam.SearchTerm
        };
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        var viewModel = new MemberViewModel
        {
            Response = result.Value,
            SearchTerm = queryParam.SearchTerm,
            SortColumn = queryParam.SortColumn,
            SortOrder = queryParam.SortOrder
        };
        return View(viewModel);
    }

    [HttpGet("SearchMember")]
    public async Task<IActionResult> SearchMember(string searchValue, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_CUSTOMER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new MemberSearchCommand(searchValue);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest();
        return Ok(result.Value);
    }

    [HttpGet("{memberId:guid}")]
    public async Task<IActionResult> Detail([FromQuery] MemberDetailQuery query, Guid memberId,
        CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_CUSTOMER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var model = new MemberDetailViewModel
        {
            Tab = string.IsNullOrEmpty(query.Tab) ? "vouchers" : query.Tab.ToLower()
        };
        if (_tabs.All(t => model.Tab.ToLower() != t)) model.Tab = "vouchers";

        var command = new GetMemberByIdCommand(new MemberId(memberId));
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        model.Member = result.Value;
        var memberVoucherCommand = new GetAllMemberVoucherPagedCommand(new MemberId(memberId))
        {
            Page = query.Page,
            PageSize = query.PageSize
        };
        var memberVouchers = await _sender.Send(memberVoucherCommand, cancellationToken);
        if (result.IsSuccess)
            model.MemberVouchers = memberVouchers.Value;
        var invoiceCommand = new GetAllInvoiceHistoryPagedCommand(new MemberId(memberId))
        {
            Page = query.Page,
            PageSize = query.PageSize
        };
        var invoiceResult = await _sender.Send(invoiceCommand, cancellationToken);
        if (invoiceResult.IsSuccess)
            model.InvoiceHistories = invoiceResult.Value;
        var memberPointHistoryCommand = new GetAllMemberPointHistoryPagedCommand(new MemberId(memberId))
        {
            Page = query.Page,
            PageSize = query.PageSize
        };
        var memberPointHistories = await _sender.Send(memberPointHistoryCommand, cancellationToken);
        if (memberPointHistories.IsSuccess)
            model.PointHistories = memberPointHistories.Value;

        return View(model);
    }

    [HttpGet("/Member/manage-member/{id:guid}")]
    [HttpGet("/Member/manage-member")]
    public async Task<IActionResult> ManageMemberView(Guid? id, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_CUSTOMER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");
        var manageMemberViewModel = new ManageMemberViewModel();
        if (!id.HasValue) return View(manageMemberViewModel);
        var memberCommand = new GetMemberByIdCommand(new MemberId(id.Value));
        var memberResult = await _sender.Send(memberCommand, cancellationToken);
        if (memberResult.IsFailure)
            return BadRequest(memberResult.Error);
        var member = memberResult.Value;
        manageMemberViewModel.Address = member.Address;
        manageMemberViewModel.Email = member.Email;
        manageMemberViewModel.BirthDate =
            member.BirthDate.HasValue ? member.BirthDate.Value.ToString("dd/MM/yyyy") : "";
        manageMemberViewModel.FirstName = member.FirstName;
        manageMemberViewModel.LastName = member.LastName;
        manageMemberViewModel.PhoneNumber = member.PhoneNumber;
        manageMemberViewModel.Id = member.Id;
        manageMemberViewModel.Note = member.Note;

        return View(manageMemberViewModel);
    }

    [HttpPost("/member/create")]
    public async Task<IActionResult> Create([FromForm] ManageMemberViewModel request,
        CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_CUSTOMER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");
        var command = new CreateMemberCommand(new FirstName(request.FirstName), new LastName(request.LastName),
            new Email(request.Email), new PhoneNumber(request.PhoneNumber), new Address(request.Address),
            string.IsNullOrEmpty(request.BirthDate) ? null : request.BirthDate.StringToDateTimeUtc());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok(result.IsSuccess);
    }

    [HttpPost("/Member/Import")]
    public async Task<IActionResult> ImportMember([FromForm] MemberFileImportRequestModel request,
        CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_CUSTOMER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");
        using var ms = new MemoryStream();
        await request.MemberFile.CopyToAsync(ms, cancellationToken);
        var command = new ImportMembersCommand(ms);
        await _sender.Send(command, cancellationToken);
        await ms.DisposeAsync();
        return Ok();
    }

    [HttpGet("/Member/Export")]
    public async Task<IActionResult> ExportMember(CancellationToken cancellationToken)
    {
        try
        {
            var checkRoleExist =
                await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_CUSTOMER", cancellationToken);
            if (!checkRoleExist.Value) return Redirect("/NoPermission");
            var command = new ExportMemberCommand();
            var result = await _sender.Send(command, cancellationToken);
            if (result.IsFailure)
                return BadRequest(result.Error);
            var currentTime = DateTime.Now.ToString("ddMMyyyyHHmmss");
            var fileName = $"{currentTime}.xlsx";
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // Return the file with the specified file name and content type
            var file = File(result.Value, contentType, fileName);
            return file;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet("/Member/Example-Import")]
    public async Task<IActionResult> GetExampleImportFile(CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_CUSTOMER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");
        var command = new GetExampleImportFileCommand();
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(result.Error);
    }

    [HttpPost("/member/update")]
    public async Task<IActionResult> Update([FromForm] ManageMemberViewModel request,
        CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_CUSTOMER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");
        if (!request.Id.HasValue)
            return BadRequest(new Error("MemberId.NotFound", "Missing member id"));
        var command = new UpdateMemberBySystemCommand
        {
            Address = request.Address,
            Email = request.Email,
            BirthDate = string.IsNullOrEmpty(request.BirthDate) ? null : request.BirthDate.StringToDateTimeUtc(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Id = new MemberId(request.Id.Value),
            Note = request.Note
        };

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok(result.IsSuccess);
    }
}