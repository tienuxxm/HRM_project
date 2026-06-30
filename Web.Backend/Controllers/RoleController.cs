using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.Permissions.GetAll;
using Application.Roles.Create;
using Application.Roles.Delete;
using Application.Roles.GetAllPaged;
using Application.Roles.GetOne;
using Application.Roles.Update;
using Domain.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;

namespace Web.Backend.Controllers
{
    
    [Authorize]
    public class RoleController : Controller
    {
        private readonly ISender _sender;
        private readonly IUserContext _userContext;
        private readonly IRoleService _roleService;

        public RoleController(IUserContext userContext, ISender sender, IRoleService roleService)
        {
            _userContext = userContext;
            _sender = sender;
            _roleService = roleService;
        }

        [HttpGet("/role")]
        public async Task<IActionResult> Index([FromQuery] PageQueryParam query, CancellationToken cancellationToken)
        {
            var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_ROLE", cancellationToken);
            if (!checkRoleExist.Value)
            {
               return  Redirect("/NoPermission");
            }
            var command = new GetAllRolePagedCommand() { Page = query.Page, PageSize = query.PageSize };

            var result = await _sender.Send(command, cancellationToken);
            return View(result.Value);
        }
        
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var command = new DeleteRoleCommand(new RoleId(id));
            var result = await _sender.Send(command, cancellationToken);
            if (result.IsFailure)
                return NoContent();
            return RedirectToAction("Index");
        }
        
        
        [HttpGet("role/{id}")]
        public async Task<IActionResult> Detail(Guid id, CancellationToken cancellationToken)
        {
            var getRoleCommand = new GetOneRoleCommand(new RoleId(id));
            var getRoleResult = await _sender.Send(getRoleCommand, cancellationToken);
            if (getRoleResult.IsFailure)
                return NotFound();
            var getPermissionCommand = new GetAllPermissionCommand(null, null,null);
            var getPermissionResult = await _sender.Send(getPermissionCommand, cancellationToken);
          
            if (getPermissionResult.IsFailure)
                return NotFound();
            var roleDetailViewModel = new ManageRoleViewModel()
            {
                ManageRoleModel = new ManageRoleModel()
                {
                    Id = getRoleResult.Value.Id,
                    DisplayName = getRoleResult.Value.DisplayName,
                    PermissionIds = getRoleResult.Value.Permissions.Select(item => item.Id).ToList(),
                },
                Permissions = getPermissionResult.Value
            };
            return View(roleDetailViewModel);
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(ManageRoleModel manageRoleModel, CancellationToken cancellationToken)
        {
            var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_ROLE", cancellationToken);
            if (!checkRoleExist.Value)
            {
                return  Redirect("/NoPermission");
            }
            var command = new CreateRoleCommand(manageRoleModel.DisplayName, manageRoleModel.DisplayName, null, manageRoleModel.PermissionIds );
            var result = await _sender.Send(command, cancellationToken);
            if (result.IsFailure)
                return BadRequest();
            return Ok();
        }
        
        [HttpGet("Role/Create")]
        public async Task<IActionResult> CreateRoleView(CancellationToken cancellationToken)
        {
            var getPermissionCommand = new GetAllPermissionCommand(null, null, null);
            var createRoleViewModel = new ManageRoleViewModel();
            var getPermissionCommandResult = await _sender.Send(getPermissionCommand, cancellationToken);
            if (getPermissionCommandResult.IsSuccess)
                createRoleViewModel.Permissions = getPermissionCommandResult.Value;
            return View(createRoleViewModel);
        }
        
        [HttpPost]
        public async Task<IActionResult> Update(ManageRoleModel model, CancellationToken cancellationToken)
        {
            var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_ROLE", cancellationToken);
            if (!checkRoleExist.Value)
            {
                return  Redirect("/NoPermission");
            }
            if (model.Id.HasValue)
            {
                var roleId = new RoleId(model.Id.Value);
                var commandGetRole = new GetOneRoleCommand(roleId);
                var role = await _sender.Send(commandGetRole, cancellationToken);
                if (role.IsFailure)
                {
                    return BadRequest(role.Error);
                }

                var command = new UpdateRoleCommand(roleId, model.DisplayName, model.DisplayName, null,
                    model.PermissionIds);
                var roleUpdated = await _sender.Send(command, cancellationToken);
                if (roleUpdated.IsFailure)
                    return BadRequest();
                return Ok();
            }
           
            return Ok();
        }
    }
}