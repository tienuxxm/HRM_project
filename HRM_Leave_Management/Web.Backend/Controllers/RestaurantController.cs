using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.FileUpload;
using Application.RestaurantArea.GetAll;
using Application.Restaurants.Create;
using Application.Restaurants.Delete;
using Application.Restaurants.GetAllPaged;
using Application.Restaurants.GetOne;
using Application.Restaurants.ToggleAvailable;
using Application.Restaurants.Update;
using Domain.RestaurantAreas;
using Domain.Restaurants;
using Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;

namespace Web.Backend.Controllers;

[Route("restaurant")]
public class RestaurantController : Controller
{
    private readonly IRoleService _roleService;
    private readonly ISender _sender;
    private readonly IUserContext _userContext;

    public RestaurantController(ISender sender, IUserContext userContext, IRoleService roleService)
    {
        _sender = sender;
        _userContext = userContext;
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] PageQueryParam query, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_RESTAURANT", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new GetAllRestaurantPagedCommand
        {
            Page = query.Page,
            PageSize = query.PageSize,
            SortColumn = nameof(Restaurant.RestaurantName),
            SortOrder = "ASC"
        };
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest();
        return View(result.Value);
    }

    /*[HttpGet]
    public async Task<JsonResult> Search([FromQuery] string value, CancellationToken cancellationToken)
    {
        var command = new MemberSearchCommand(value);
        var result = await _sender.Send(command, cancellationToken);
        return Json(result.IsFailure ? null : result.Value);
    }*/

    [HttpGet("manage")]
    public async Task<IActionResult> ManageRestaurant(Guid? id, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_RESTAURANT", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var restaurantAreaCommand = new GetAllRestaurantAreaCommand();
        var restaurantAreas = await _sender.Send(restaurantAreaCommand, cancellationToken);


        var restaurantViewModel = new ManageRestaurantViewModel
        {
            RestaurantAreas = restaurantAreas.Value,
            PageTitle = "Add branch",
            ManageRestaurantModel = new ManageRestaurantModel()
        };
        if (id.HasValue)
        {
            var restaurantCommand = new GetRestaurantDetailCommand(id.Value);
            var restaurant = await _sender.Send(restaurantCommand, cancellationToken);
            if (restaurant.IsFailure)
                return NotFound();

            restaurantViewModel.PageTitle = "Update branch";
            restaurantViewModel.ManageRestaurantModel = new ManageRestaurantModel
            {
                Id = id,
                City = restaurant.Value.Address.City,
                Country = restaurant.Value.Address.Country,
                State = restaurant.Value.Address.State,
                Street = restaurant.Value.Address.Street,
                ZipCode = restaurant.Value.Address.ZipCode,
                AreaId = restaurant.Value.AreaId,
                ClosingAt = restaurant.Value.ClosingAt,
                OpeningAt = restaurant.Value.OpeningAt,
                ImageUrl = restaurant.Value.ImageUrl ?? "",
                RestaurantName = restaurant.Value.RestaurantName,
                MapLink = restaurant.Value.MapLink
            };
        }

        return View(restaurantViewModel);
    }

    [HttpPost("/Restaurant/ToggleAvailable")]
    public async Task<IActionResult> ToggleAvailable([FromForm] RestaurantToggleAvailableRequestBody body,
        CancellationToken cancellationToken)
    {
        var command = new ToggleAvailableRestaurantCommand(new RestaurantId(body.Id), body.Toggle);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok(result.IsSuccess);
    }


    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] ManageRestaurantModel model, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_RESTAURANT", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var imageKey = model.ImageFile != null ? Guid.NewGuid().ToString() : null;
        if (model.ImageFile != null && !string.IsNullOrEmpty(imageKey))
            using (var ms = new MemoryStream())
            {
                await model.ImageFile.CopyToAsync(ms, cancellationToken);
                var fileCommand = new FileUploadCommand(ms, imageKey);
                await _sender.Send(fileCommand, cancellationToken);
            }

        var command = new CreateRestaurantCommand(new RestaurantName(model.RestaurantName),
            new Address("Vietnamese", model.State, model.ZipCode, model.City, model.Street), model.OpeningAtValue,
            model.ClosingAtValue, model.AreaId.HasValue ? new RestaurantAreaId(model.AreaId.Value) : null,
            new ImageUrl(imageKey ?? ""), model.MapLink);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return NoContent();
        return RedirectToAction("Index");
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromForm] UpdateRestaurantModel model, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_RESTAURANT", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var imageKey = model.ImageFile != null ? Guid.NewGuid().ToString() : null;
        if (model.ImageFile != null && !string.IsNullOrEmpty(imageKey))
            using (var ms = new MemoryStream())
            {
                await model.ImageFile.CopyToAsync(ms, cancellationToken);
                var fileCommand = new FileUploadCommand(ms, imageKey);
                await _sender.Send(fileCommand, cancellationToken);
            }

        var command = new UpdateRestaurantCommand(
            new RestaurantId(model.Id),
            new RestaurantName(model.RestaurantName),
            new Address("Vietnamese", model.State, model.ZipCode, model.City, model.Street), model.OpeningAtValue,
            model.ClosingAtValue, model.AreaId.HasValue ? new RestaurantAreaId(model.AreaId.Value) : null,
            string.IsNullOrEmpty(imageKey) ? null : new ImageUrl(imageKey), model.MapLink);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return NoContent();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_RESTAURANT", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new DeleteRestaurantCommand(new RestaurantId(id));
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
            return RedirectToAction("Index");
        return NoContent();
    }
}