using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.FileUpload;
using Application.Promotions.Create;
using Application.Promotions.Delete;
using Application.Promotions.GetAllPaged;
using Application.Promotions.GetOne;
using Application.Promotions.Update;
using Domain.Promotions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;

namespace Web.Backend.Controllers;

public class PromotionController : Controller
{
    private readonly ISender _sender;
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;

    public PromotionController(ISender sender, IUserContext userContext, IRoleService roleService)
    {
        _sender = sender;
        _userContext = userContext;
        _roleService = roleService;
    }


    // GET
    [HttpGet()]
    public async Task<IActionResult> Index([FromQuery] PageQueryParam query, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_PROMOTION", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        var command = new GetAllPagedPromotionsCommand()
        {
            Page = query.Page,
            PageSize = query.PageSize
        };
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return View(result.Value);
    }

    public async Task<IActionResult> ManagedPromotionView(Guid? id, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_PROMOTION", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        if (id.HasValue)
        {
            Guid nonNullableGuid = id.Value;
            var command = new GetPromotionCommand(new PromotionId(nonNullableGuid));
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            if (result.IsSuccess)
            {
                return View(new ManagedPromotionViewModel()
                {
                    TitlePage = "Edit Promotion",
                    Id = result.Value.Id,
                    Title = result.Value.Title,
                    Content = result.Value.Content,
                    StartedAt = result.Value.StartedAt.ToString("dd/MM/yyyy"),
                    EndedAt = result.Value.EndedAt.ToString("dd/MM/yyyy"),
                    ImageUrl = result.Value.ImageUrl,
                    PromotionName = result.Value.PromotionName
                });
            }
        }

        return View(new ManagedPromotionViewModel()
        {
            TitlePage = "Add Promotion",
            StartedAt = DateTime.Now.ToString("dd/MM/yyyy"),
            EndedAt = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy"),
        });
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromForm] ManagedPromotionViewModel model,
        CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_PROMOTION", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        var imageKey = Guid.NewGuid().ToString();
        using (var ms = new MemoryStream())
        {
            await model.Image.CopyToAsync(ms, cancellationToken);
            var fileCommand = new FileUploadCommand(ms, imageKey);
            await _sender.Send(fileCommand, cancellationToken);
        }

        var command =
            new CreatePromotionCommand(
                model.PromotionName,
                model.Content,
                model.Title,
                model.StartedAtUtc,
                model.EndedAtUtc,
                imageKey
            );
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
            return RedirectToAction("Index");
        return NoContent();
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
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_USER", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        var command = new GetAllPagedPromotionsCommand()
        {
            Page = (startValue + 10) / 10,
            PageSize = lengthValue > 0 ? lengthValue : 10
        };

        var columnOrder = column.ToString() switch
        {
            "2" => nameof(Promotion.Content),
            "3" => nameof(Promotion.CreatedDate),
            _ => null
        };

        if (!string.IsNullOrEmpty(search))
        {
            command.SearchTerm = search.ToString().Trim();
        }

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

    [HttpPost]
    public async Task<IActionResult> Update([FromForm] ManagedPromotionViewModel model,
        CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_PROMOTION", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        if (model.Id.HasValue)
        {
            var getOneNewsCommand = new GetPromotionCommand(new PromotionId(model.Id.Value));
            var promotion = await _sender.Send(getOneNewsCommand, cancellationToken);
            if (promotion.IsFailure)
                return BadRequest(promotion.Error);

            var imageKey = model.Image != null ? Guid.NewGuid().ToString() : null;
            if (model.Image != null)
            {
                using (var ms = new MemoryStream())
                {
                    await model.Image.CopyToAsync(ms, cancellationToken);
                    var fileCommand = new FileUploadCommand(ms, imageKey);
                    await _sender.Send(fileCommand, cancellationToken);
                }
            }

            var command = new UpdatePromotionCommand(
                new PromotionId(model.Id.Value),
                model.PromotionName,
                model.Content,
                model.Title,
                imageKey,
                model.StartedAtUtc,
                model.EndedAtUtc
            );
            var result = await _sender.Send(command, cancellationToken);
            if (result.IsSuccess)
                return RedirectToAction("Index");
        }

        return NoContent();
    }

    [HttpPost("/Promotion/Delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_PROMOTION", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        var command = new DeletePromotionCommand(new PromotionId(id));
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
            return RedirectToAction("Index");
        return NoContent();
    }
}