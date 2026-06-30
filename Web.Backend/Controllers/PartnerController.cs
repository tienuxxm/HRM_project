using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.Partners.Create;
using Application.Partners.Delete;
using Application.Partners.GenerateQrCode;
using Application.Partners.GetAllPaged;
using Application.Partners.GetOne;
using Application.Partners.Update;
using Domain.Partners;
using Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;

namespace Web.Backend.Controllers;

[Authorize]
public class PartnerController : Controller
{
    private readonly ISender _sender;
    private readonly IUserContext _userContext;

    private readonly IRoleService _roleService;

    // GET
    public PartnerController(ISender sender, IRoleService roleService, IUserContext userContext)
    {
        _sender = sender;
        _roleService = roleService;
        _userContext = userContext;
    }

    public async Task<IActionResult> Index([FromQuery] PageQueryParam query, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_PARTNER", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        var getPartnersCommand = new GetAllPartnerPagedCommand()
        {
            Page = query.Page,
            PageSize = query.PageSize,
        };
        var getPartnerResult = await _sender.Send(getPartnersCommand, cancellationToken);
        if (getPartnerResult.IsFailure)
            return BadRequest();
        return View(getPartnerResult.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] ManagePartnerViewModel model,
        CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_PARTNER", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        var command = new CreatePartnerCommand(new PartnerName(model.PartnerName),
            model.Address != null ? new PartnerAddress(model.Address) : null,
            model.PhoneNumber != null ? new PhoneNumber(model.PhoneNumber) : null,
            model.Email != null ? new Email(model.Email) : null);
        var resutl = await _sender.Send(command, cancellationToken);
        if (resutl.IsFailure)
            return NoContent();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Update([FromForm] ManagePartnerViewModel model,
        CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_PARTNER", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        if (model.id is not null)
        {
            var command = new UpdatePartnerCommand(
                new PartnerId(model.id.Value),
                model.PartnerName,
                model.Email,
                model.PhoneNumber,
                model.Address,
                null
            );
            var resutl = await _sender.Send(command, cancellationToken);
            if (resutl.IsFailure)
                return NoContent();
        }
        else
        {
            return NoContent();
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> ManagePartnerView(Guid? id, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_PARTNER", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        if (id is not null)
        {
            var command = new GetOnePartnerCommand(
                new PartnerId(id.Value));
            var resutl = await _sender.Send(command, cancellationToken);
            if (resutl.IsFailure)
                return NoContent();

            return View(new ManagePartnerViewModel()
            {
                id = id,
                title = "Update Partner",
                Address = resutl.Value.Address,
                Email = resutl.Value.Email,
                PhoneNumber = resutl.Value.PhoneNumber,
                PartnerName = resutl.Value.PartnerName,
            });
        }
        else
        {
            return View(new ManagePartnerViewModel()
            {
                title = "Add Partner"
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_PARTNER", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        var command = new DeletePartnerCommand(new PartnerId(id));
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
            return RedirectToAction("Index");
        return NoContent();
    }

    public async Task<IActionResult> ReGenerateQrCode(Guid id, CancellationToken cancellationToken)
    {
        var comamnd = new ReGeneratePartnerQrCodeCommand(id);
        var result = await _sender.Send(comamnd, cancellationToken);
        if (result.IsFailure)
            return NoContent();
        return RedirectToAction("Index");
    }
}