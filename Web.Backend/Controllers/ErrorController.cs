using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Backend.Controllers;

[Authorize]
public class ErrorController : Controller
{
    // GET
    [HttpGet("/NotFound")]
    public IActionResult NotFound404()
    {
        return View();
    }
    
    [HttpGet("/NoPermission")]
    public IActionResult NoPermission()
    {
        return View();
    }
}