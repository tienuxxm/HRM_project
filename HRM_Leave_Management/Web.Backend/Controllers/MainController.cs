using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;

namespace Web.Backend.Controllers;

public class MainController : Controller
{
    // GET
    [HttpGet("SuccessToast")]
    public IActionResult SuccessToastPartial(string message)
    {
        return PartialView("_Toast", new ToastViewModel()
        {
            Message = message,
            Type = ToastType.Success
        });
    }

    [HttpGet("FailToast")]
    public IActionResult FailToastPartial(string message)
    {
        return PartialView("_Toast", new ToastViewModel()
        {
            Message = message,
            Type = ToastType.Error
        });
    }
}