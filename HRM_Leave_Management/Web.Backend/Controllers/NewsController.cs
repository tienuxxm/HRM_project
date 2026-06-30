using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.FileUpload;
using Application.News.Create;
using Application.News.Delete;
using Application.News.GetAllPaged;
using Application.News.GetOne;
using Application.News.Update;
using Domain.News;
using Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;

namespace Web.Backend.Controllers;

[Authorize]
public class NewsController : Controller
{
    private readonly IRoleService _roleService;
    private readonly ISender _sender;
    private readonly IUserContext _userContext;

    public NewsController(ISender sender, IUserContext userContext, IRoleService roleService)
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
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_NEWS", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new GetAllNewsPagedCommand
        {
            Page = (startValue + 10) / 10,
            PageSize = lengthValue > 0 ? lengthValue : 10
        };

        var columnOrder = column.ToString() switch
        {
            "2" => nameof(News.Content),
            "3" => nameof(News.CreatedDate),
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
    public async Task<IActionResult> Index([FromQuery] PageQueryParam query, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_NEWS", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new GetAllNewsPagedCommand
        {
            Page = query.Page,
            PageSize = query.PageSize
        };
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return View(result.Value);
    }

    public async Task<IActionResult> ManagedNewsView(Guid? id, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_NEWS", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        if (id.HasValue)
        {
            var nonNullableGuid = id.Value;
            var command = new GetOneNewsCommand(new NewsId(nonNullableGuid));
            var result = await _sender.Send(command, cancellationToken);
            if (result.IsSuccess)
                return View(new ManagedNewsViewModel
                {
                    Id = result.Value.Id,
                    TitlePage = "Edit News",
                    Title = result.Value.Title,
                    Content = result.Value.Content,
                    Description = result.Value.Description,
                    ThumbnailUrl = result.Value.Thumbnail
                });
        }

        return View(new ManagedNewsViewModel
        {
            TitlePage = "Add News"
        });
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromForm] ManagedNewsViewModel model,
        CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_NEWS", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var thumbnailKey = Guid.NewGuid().ToString();
        using (var ms = new MemoryStream())
        {
            await model.Thumbnail.CopyToAsync(ms, cancellationToken);
            var fileCommand = new FileUploadCommand(ms, thumbnailKey);
            await _sender.Send(fileCommand, cancellationToken);
        }

        var command =
            new CreateNewsCommand(
                new Content(model.Content),
                new Title(model.Title),
                new Description(model.Description),
                new ImageUrl(thumbnailKey)
            );
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
            return RedirectToAction("Index");
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Update([FromForm] ManagedNewsViewModel model, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_NEWS", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        if (model.Id.HasValue)
        {
            var getOneNewsCommand = new GetOneNewsCommand(new NewsId(model.Id.Value));
            var news = await _sender.Send(getOneNewsCommand, cancellationToken);
            if (news.IsFailure)
                return BadRequest(news.Error);

            var thumbnailKey = model.Thumbnail != null ? Guid.NewGuid().ToString() : news.Value.ThumbNailId;
            if (model.Thumbnail != null)
                using (var ms = new MemoryStream())
                {
                    await model.Thumbnail.CopyToAsync(ms, cancellationToken);
                    var fileCommand = new FileUploadCommand(ms, thumbnailKey);
                    await _sender.Send(fileCommand, cancellationToken);
                }

            var command = new UpdateNewsCommand(
                new NewsId(model.Id.Value),
                new Content(model.Content),
                new Title(model.Title),
                new Description(model.Description),
                new ImageUrl(thumbnailKey)
            );
            var result = await _sender.Send(command, cancellationToken);
            if (result.IsSuccess)
                return RedirectToAction("Index");
        }

        return NoContent();
    }

    [HttpPost("/News/Delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_NEWS", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new DeleteNewsCommand(new NewsId(id));
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
            return Ok();
        return BadRequest();
    }
}