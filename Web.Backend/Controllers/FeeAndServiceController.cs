using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.FeeServices.Create;
using Application.FeeServices.Delete;
using Application.FeeServices.GetAllPaged;
using Application.FeeServices.GetOne;
using Application.FeeServices.Update;
using Domain.FreeServices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;

namespace Web.Backend.Controllers;

[Route("fee-and-services")]
[Authorize]
public class FeeAndServiceController : Controller
{
    private readonly ISender _sender;
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;
    public FeeAndServiceController(ISender sender, IUserContext userContext, IRoleService roleService)
    {
        _sender = sender;
        _userContext = userContext;
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] PageQueryParam query, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_FEE_AND_SERVICE", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return  Redirect("/NoPermission");
        }
        var command = new GetAllFeeAndServicePagedCommand()
        {
            Page = query.Page,
            PageSize = query.PageSize
        };
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest();

        return View(result.Value);
    }

    [HttpGet("fee-and-service/create")]
    public async Task<IActionResult> FeeAndServiceCreateView(CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_FEE_AND_SERVICE", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return  Redirect("/NoPermission");
        }
        return View(new CreateOrUpdateFeeAndServiceViewModel());
    }
    
    [HttpGet("fee-and-service/update/{id}")]
    public async Task<IActionResult> FeeAndServiceUpdateView(Guid id , CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_FEE_AND_SERVICE", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return  Redirect("/NoPermission");
        }
        var commandFeeAndServices = new GetOneFeeServicesCommand(new FeeServiceId(id));
        var result = await _sender.Send(commandFeeAndServices, cancellationToken);
        if (result.IsFailure)
            return BadRequest();
        return View(new UpdateFeeAndServiceViewModel()
        {
            UpdateFeeAndServiceModel = new UpdateFeeAndServiceModel()
            {
                Id = result.Value.Id,
                FeeAndServiceName = result.Value.FeeAndServiceName,
                FeePercent  = result.Value.FeePercent,
                FeeAmount = result.Value.FeeAmount != null ? result.Value.FeeAmount.Amount: null ,
                IsPercent  = result.Value.IsPercent,
                IsActive  = result.Value.IsActive,
                FeeType  = (int)result.Value.FeeType,
            }
        });
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromForm] CreateOrUpdateFeeAndServiceViewModel request,
        CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_FEE_AND_SERVICE", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return  Redirect("/NoPermission");
        }
        var command = new CreateFeeServiceCommand(request.CreateFeeAndServiceModels.Select(x =>
            new CreateFeeServiceRequest()
            {
                FeeName = x.FeeAndServiceName,
                IsPercent = x.IsPercent,
                FeeAmount = x.IsPercent ? null : x.FeeValue,
                FeePercent = x.IsPercent ? (float)x.FeeValue : null,
                FeeType = (FeeType)x.FeeType,
                IsActive = x.IsActive
            }).ToList());
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok();
    }
    
    [HttpPost("Update")]
    public async Task<IActionResult> Update([FromForm] CreateFeeAndServiceModel request,
        CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_FEE_AND_SERVICE", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return  Redirect("/NoPermission");
        }
        if (request.Id.HasValue)
        {
            var command = new UpdateFeeServicesCommand(
                request.Id.Value,
                request.FeeAndServiceName,
                request.IsPercent ? null : request.FeeValue,
                request.IsPercent ? (float)request.FeeValue: null,
                request.IsPercent,
                (FeeType)request.FeeType,
                request.IsActive
            );
            var result = await _sender.Send(command, cancellationToken);
            if (result.IsFailure)
                return BadRequest(result.Error);
        }
        return RedirectToAction("Index");
    }
    
    [HttpPost]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_FEE_AND_SERVICE", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return  Redirect("/NoPermission");
        }
        var command = new DeleteFeeServicesCommand(new FeeServiceId(id));
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
            return RedirectToAction("Index");
        return NoContent();
    }
}