using Application.Products.GetAll;
using Application.Restaurants.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;

namespace Web.Backend.Controllers;

[Authorize]
[Route("product-restaurant")]
public class ProductRestaurantController : Controller
{
    private readonly ISender _sender;

    // GET
    public ProductRestaurantController(ISender sender)
    {
        _sender = sender;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromForm] CreateProductRestaurantModel model,
        CancellationToken cancellationToken)
    {
        return Ok();
    }

    [HttpGet("manage-product-restaurant")]
    public async Task<IActionResult> ManageProductRestaurantView(Guid? bookingId, CancellationToken cancellationToken)
    {
        /*var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_BOOKING", cancellationToken);
        if (!checkRoleExist.Value)
            return Redirect("/NoPermission");
        }*/

        var getAllRestaurantCommand = new GetAllRestaurantCommand();
        var getAllRestaurantResult = await _sender.Send(getAllRestaurantCommand, cancellationToken);
        if (getAllRestaurantResult.IsFailure)
            return BadRequest();
        var getAllProductCommand = new GetAllProductCommand();
        var getAllProductResult = await _sender.Send(getAllProductCommand, cancellationToken);
        if (getAllProductResult.IsFailure)
            return BadRequest();

        var manageViewModel = new ManageProductRestaurantViewModel
        {
            Restaurants = getAllRestaurantResult.Value,
            Products = getAllProductResult.Value,
            PageTitle = "Add branch menu"
        };
        /*if (bookingId.HasValue)
        {
            var getBookingCommand = new GetBookingCommand(bookingId.Value);
            var bookingResult = await _sender.Send(getBookingCommand, cancellationToken);
            if (bookingResult.IsFailure)
                return Redirect("/NotFound");
            var booking = bookingResult.Value;
            manageBookingViewModel.Title = "Update Reservation";
            manageBookingViewModel.BookingModel = new BookingModel()
            {
                Id = booking.Id,
                Date = booking.BookingTime.ToString("dd/MM/yyyy"),
                Time = booking.BookingTime.ToString("HH:mm"),
                FullName = booking.MemberName,
                PhoneNumber = booking.PhoneNumber ?? "",
                NumberOfPeople = booking.TotalOfPeople,
                RestaurantId = booking.RestaurantId,
                Note = booking.Note,
                LineItems = booking.LineItemResponses?.Select(l => new BookingLineItemModel()
                {
                    Note = l.Note,
                    Quantity = l.Quantity,
                    ProductId = l.ProductId
                }).ToList()
            };
        }
        */

        return View(manageViewModel);
    }
}