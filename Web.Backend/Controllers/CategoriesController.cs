using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.Categories.Create;
using Application.Categories.Delete;
using Application.Categories.GetAllPaged;
using Application.Categories.GetOne;
using Application.Categories.Update;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;

namespace Web.Backend.Controllers;

[Authorize]
public class CategoriesController : Controller
{
    // GET
    private readonly ISender _sender;
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;

    public CategoriesController(ISender sender, IUserContext userContext, IRoleService roleService)
    {
        _sender = sender;
        _userContext = userContext;
        _roleService = roleService;
    }

    public async Task<IActionResult> Categories([FromQuery] PageQueryParam query, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_CATEGORY", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        var command = new GetAllCategoryQuery() { Page = query.Page, PageSize = query.PageSize };
        var result = await _sender.Send(command, cancellationToken);
        return View(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateCategoryViewModel model,
        CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_CATEGORY", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        var command = new CreateCategoryCommand(model.CategoryName, model.Description, model.Index);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
            return RedirectToAction("Categories");
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Update([FromForm] CreateCategoryViewModel model,
        CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_CATEGORY", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        if (model.Id.HasValue)
        {
            var command = new UpdateCategoryCommand(model.Id.Value, model.CategoryName, model.Description, model.Index);
            var result = await _sender.Send(command, cancellationToken);
            if (result.IsSuccess)
                return RedirectToAction("Categories");
            return NoContent();
        }

        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_CATEGORY", cancellationToken);
        if (!checkRoleExist.Value)
        {
            return Redirect("/NoPermission");
        }

        var command = new DeleteCategoryCommand() { Id = id };
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
            return RedirectToAction("Categories");
        return NoContent();
    }

    public IActionResult DeleteCategoryView(CategoryResponse category)
    {
        return PartialView("_DeleteCategory", category);
    }
}