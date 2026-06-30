using Application.FileUpload;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;

namespace Web.Backend.Controllers;

[Route("Upload")]
public class UploadController : Controller
{
    private readonly ISender _sender;

    public UploadController(ISender sender)
    {
        _sender = sender;
    }

    // GET
    [HttpPost("ImageUpload")]
    public async Task<IActionResult> ImageUpload([FromForm] UploadRequest request, CancellationToken cancellationToken)
    {
        var imageKey = Guid.NewGuid().ToString();
        using (var ms = new MemoryStream())
        {
            await request.Image.CopyToAsync(ms, cancellationToken);
            var fileCommand = new FileUploadCommand(ms, imageKey);
            var result = await _sender.Send(fileCommand, cancellationToken);
            if (result.IsFailure)
                return BadRequest();
            return Ok(new
            {
                Success = 1,
                File = new
                {
                    Url = result.Value
                }
            });
        }

        return BadRequest();
    }
}