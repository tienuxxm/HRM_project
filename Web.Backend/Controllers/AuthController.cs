using Application.Auth.GetUserInfo;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Backend.Controllers;

[Authorize]
[Route("Auth")]
public class AuthController : Controller
{
    private readonly ISender _sender;

    // GET
    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("GetUserLogin")]
    public async Task<IActionResult> GetUserLogin(CancellationToken cancellationToken)
    {
        var command = new GetUserInfoCommand();
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpGet("Logout")]
    public IActionResult Logout(CancellationToken cancellationToken)
    {
        Response.Cookies.Delete("X-Access-Token");
        return Redirect("/Auth/Login");
    }
}