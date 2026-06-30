using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.Roles.GetAll;
using Application.Users.Create;
using Application.Users.Delete;
using Application.Users.GetAllPaged;
using Application.Users.GetOne;
using Application.Users.Update;
using Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;

namespace Web.Backend.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly ILogger<UserController> _logger;
    private readonly IRoleService _roleService;
    private readonly ISender _sender;
    private readonly IUserContext _userContext;

    public UserController(ILogger<UserController> logger, ISender sender, IUserContext userContext,
        IRoleService roleService)
    {
        _sender = sender;
        _userContext = userContext;
        _roleService = roleService;
        _logger = logger;
        _sender = sender;
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
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new GetAllUserPagedCommand
        {
            Page = (startValue + 10) / 10,
            PageSize = lengthValue > 0 ? lengthValue : 10
        };

        var columnOrder = column.ToString() switch
        {
            "1" => nameof(Domain.Users.User.Name),
            "2" => nameof(Domain.Users.User.Username),
            "3" => nameof(Domain.Users.User.Email),
            "4" => nameof(Domain.Users.User.Roles),
            _ => null
        };

        if (!string.IsNullOrEmpty(search)) command.SearchTerm = search;

        if (!string.IsNullOrEmpty(columnOrder))
        {
            command.SortColumn = columnOrder;
            command.SortOrder = order.ToString().ToUpper();
        }
        else
        {
            command.SortColumn = "CreatedAt";
            command.SortOrder = "DESC";
        }

        var result = await _sender.Send(command, cancellationToken);


        var jsonData = new
        {
            data = result.Value.Data,
            draw = Convert.ToInt32(draw),
            recordsFiltered = result.Value.TotalCount,
            recordsTotal = result.Value.Data.Count,
            pages = Math.Round((double)result.Value.TotalCount / lengthValue, MidpointRounding.AwayFromZero)
        };

        return Ok(jsonData);
    }

    [HttpGet("/user")]
    public async Task<IActionResult> Index([FromQuery] PageQueryParam query, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_USER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new GetAllUserPagedCommand { Page = query.Page, PageSize = query.PageSize };

        var result = await _sender.Send(command, cancellationToken);
        return View(result.Value);
    }

    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteUserCommand(new UserId(id));
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return NoContent();
        return RedirectToAction("Index");
    }

    [HttpPost("/User/Delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, string? url, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_USER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new DeleteUserCommand(new UserId(id));
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess) return string.IsNullOrEmpty(url) ? RedirectToAction("Index") : Redirect(url);

        return NoContent();
    }

    [HttpGet("user/{id}")]
    public async Task<IActionResult> Detail(Guid id, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_USER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var getUserCommand = new GetOneUserQuery(new UserId(id));
        var getUserResult = await _sender.Send(getUserCommand, cancellationToken);
        if (getUserResult.IsFailure)
            return NotFound();
        var getRoleCommand = new GetAllRoleCommand(100, 0, null);
        var getRoleCommandResult = await _sender.Send(getRoleCommand, cancellationToken);
        if (getRoleCommandResult.IsFailure)
            return NotFound();
        var userDetailViewModel = new ManageUserViewModel
        {
            ManageUserModel = new ManageUserModel
            {
                Username = getUserResult.Value.Username,
                Id = getUserResult.Value.Id,
                Email = getUserResult.Value.Email,
                Name = getUserResult.Value.Fullname,
                RoleIds = getUserResult.Value.Roles.Select(item => item.Id).ToList()
            },
            Roles = getRoleCommandResult.Value
        };
        return View(userDetailViewModel);
    }


    [HttpGet("User/modal-edit-user/{id}")]
    public async Task<IActionResult> GetContentModalEditUserById(Guid id, CancellationToken cancellationToken)
    {
        var command = new GetOneUserQuery(new UserId(id));
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure) return BadRequest(result.Error);

        var getAllRoleCommand = new GetAllRoleCommand(null, null, null);
        var resultGetAllRole = await _sender.Send(getAllRoleCommand, cancellationToken);

        if (resultGetAllRole.IsFailure) return BadRequest(result.Error);


        return PartialView("_EditUserModal",
            new ModalUserModel { User = result.Value, Roles = resultGetAllRole.Value });
    }

    [HttpPost]
    public async Task<IActionResult> Create(ManageUserModel manageUserModel, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_USER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new CreateUserCommand(manageUserModel.Name, manageUserModel.Username, manageUserModel.Password,
            manageUserModel.RoleIds,
            manageUserModel.Email, null);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok();
    }

    [HttpGet("User/Create")]
    public async Task<IActionResult> CreateUserView(CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_USER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var getRoleCommand = new GetAllRoleCommand(100, 0, null);
        var createUserViewModel = new ManageUserViewModel();
        var getRoleCommandResult = await _sender.Send(getRoleCommand, cancellationToken);
        if (getRoleCommandResult.IsSuccess)
            createUserViewModel.Roles = getRoleCommandResult.Value;

        return View(createUserViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Update(ManageUserModel model, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_USER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        if (model.Id.HasValue)
        {
            var userId = new UserId(model.Id.Value);
            var commandGetUser = new GetOneUserQuery(userId);
            var user = await _sender.Send(commandGetUser, cancellationToken);
            if (user.IsFailure) return BadRequest(user.Error);

            var command = new UpdateUserCommand(userId, model.Name, model.Email, user.Value.PhoneNumber,
                model.RoleIds);
            var userUpdated = await _sender.Send(command, cancellationToken);
            if (userUpdated.IsFailure) return BadRequest(userUpdated.Error);

            return RedirectToAction("Index", "User");
        }

        return NoContent();
    }
}