using Application.Users.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;

namespace Web.Backend.Controllers;

[Route("auth")]
public class LoginController : Controller
{
    private readonly ISender _sender;

    public LoginController(ISender sender)
    {
        _sender = sender;
    }

    // GET
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginViewModel body, CancellationToken cancellationToken)
    {
        var userLoginCommand = new AdminLoginCommand(body.Username, body.Password);
        var accessTokenResponse = await _sender.Send(userLoginCommand, cancellationToken);
        if (accessTokenResponse.IsFailure)
            return BadRequest(accessTokenResponse.Error);
        Response.Cookies.Append("X-Access-Token", accessTokenResponse.Value.AccessToken,
            new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict });
        return Ok();
    }

    [HttpGet("login-screen")]
    public IActionResult LoginScreen()
    {
        return View(new LoginViewModel());
    }
}