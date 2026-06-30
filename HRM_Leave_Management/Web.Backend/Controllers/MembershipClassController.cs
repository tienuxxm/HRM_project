using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.MembershipClasses.Create;
using Application.MembershipClasses.GetAll;
using Application.MembershipClasses.GetOne;
using Application.MembershipClasses.Update;
using Domain.MembershipClasses;
using Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;

namespace Web.Backend.Controllers;

[Route("membership-class")]
[Authorize]
public class MembershipClassController : Controller
{
    private readonly ISender _sender;
    private readonly IUserContext _userContext;

    private readonly IRoleService _roleService;

    // GET
    public MembershipClassController(ISender sender, IUserContext userContext, IRoleService roleService)
    {
        _sender = sender;
        _userContext = userContext;
        _roleService = roleService;
    }

    [HttpGet()]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_RANK", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        var command = new GetAllMembershipClassCommand();
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return View(result.Value);
    }

    [HttpGet("Manage")]
    public async Task<IActionResult> ManageMembershipView(Guid? id, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_RANK", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        if (id.HasValue)
        {
            var getOneMemberShipClassCommand = new GetOneMembershipClassCommand(new MembershipClassId(id.Value));
            var result = await _sender.Send(getOneMemberShipClassCommand, cancellationToken);
            if (result.IsFailure)
                return BadRequest(result.Error);
            return View(new ManageMembershipViewModel()
            {
                Id = result.Value.Id,
                ClassName = result.Value.ClassName,
                Level = result.Value.Level,
                MaxMoney = result.Value.MaxMoney.Amount,
                PercentDefault = result.Value.PercentDefault,
                PercentBirthDate = result.Value.PercentBirthDate,
                Benefits = result.Value.MembershipBenefits,
                EffectiveYears = result.Value.EffectiveYears
            });
        }
        else
        {
            return View(new ManageMembershipViewModel()
            {
                Benefits = new List<MembershipBenefitResponse>()
            });
        }
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromForm] ManageMembershipViewModel request,
        CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_RANK", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        var command = new CreateMembershipClassCommand(
            new ClassName(request.ClassName),
            new Level(request.Level),
            new Money(request.MaxMoney, Currency.Vnd),
            request.Benefits.Select(f =>
                new MembershipBenefitRequestCommand()
                {
                    Title = new Title(f.Title),
                    Description = !string.IsNullOrEmpty(f.Description) ? new Description(f.Description) : null
                }).ToList(),
            request.PercentDefault,
            request.PercentBirthDate,
            request.EffectiveYears
        );
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest();
        return Ok();
    }

    [HttpPost("Update")]
    public async Task<IActionResult> Update([FromForm] ManageMembershipViewModel request,
        CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_RANK", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        if (request.Id.HasValue)
        {
            var command = new UpdateMembershipClassCommand(
                new MembershipClassId(request.Id.Value),
                new ClassName(request.ClassName),
                new Level(request.Level),
                new Money(request.MaxMoney,
                    Currency.Vnd),
                request.Benefits.Select(f =>
                    new MembershipBenefitRequestCommand()
                    {
                        Title = new Title(f.Title),
                        Description = !string.IsNullOrEmpty(f.Description) ? new Description(f.Description) : null
                    }).ToList(),
                request.PercentDefault,
                request.PercentBirthDate, request.EffectiveYears);
            var result = await _sender.Send(command, cancellationToken);
            if (result.IsFailure)
                return BadRequest();
        }
        else
        {
            return BadRequest();
        }

        return Ok();
    }
}