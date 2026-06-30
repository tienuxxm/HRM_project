using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Abstractions.Role;
using Application.Bookings.CancelBooking;
using Application.Bookings.CompleteBooking;
using Application.Bookings.ConfirmBooking;
using Application.Bookings.GetAllPaged;
using Application.Bookings.GetBooking;
using Application.Bookings.RejectBooking;
using Application.Bookings.ReserveBooking;
using Application.Bookings.UpdateBooking;
using Application.Members.GetAll;
using Application.Orders.Create;
using Application.Products.GetAll;
using Application.Restaurants.GetAll;
using Domain.Bookings;
using Domain.Extension;
using Domain.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;

namespace Web.Backend.Controllers;

[Authorize]
public class BookingController : Controller
{
    private readonly IRoleService _roleService;
    private readonly ISender _sender;
    private readonly IUserContext _userContext;

    // GET
    public BookingController(ISender sender, IUserContext userContext, IRoleService roleService)
    {
        _sender = sender;
        _userContext = userContext;
        _roleService = roleService;
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
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_BOOKING", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new GetAllBookingPagedCommand(null)
        {
            Page = (startValue + 10) / 10,
            PageSize = lengthValue > 0 ? lengthValue : 10
        };

        var columnOrder = column.ToString() switch
        {
            "0" => nameof(Booking.BookingCode),
            "1" => nameof(Booking.Member.FullName),
            "2" => nameof(Booking.BookingTime),
            "3" => nameof(Booking.CreateDate),
            "4" => nameof(Booking.TotalOfPeople),
            "5" => nameof(Booking.Status),
            _ => null
        };

        if (!string.IsNullOrEmpty(search)) command.SearchTerm = search.ToString().Trim();

        if (!string.IsNullOrEmpty(columnOrder))
        {
            command.SortColumn = columnOrder;
            command.SortOrder = order.ToString().ToUpper();
        }

        var result = await _sender.Send(command, cancellationToken);


        var jsonData = new
        {
            draw = Convert.ToInt32(draw),
            recordsFiltered = result.Value.TotalCount,
            recordsTotal = result.Value.Data.Count,
            data = result.Value.Data,
            pages = Math.Round((double)result.Value.TotalCount / lengthValue, MidpointRounding.AwayFromZero)
        };

        return Ok(jsonData);
    }

    [HttpGet("Booking")]
    public async Task<IActionResult> Index([FromQuery] PageQueryParam queryParam, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_BOOKING", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var viewModel = new BookingViewModel
        {
            SearchTerm = queryParam.SearchTerm,
            SortColumn = queryParam.SortColumn,
            SortOrder = queryParam.SortOrder
        };

        var command = new GetAllBookingPagedCommand(null)
        {
            Page = queryParam?.Page ?? 1,
            PageSize = queryParam?.PageSize ?? 10,
            SearchTerm = queryParam?.SearchTerm
        };
        var result = await _sender.Send(command, cancellationToken);
        if (result.Error.Equals(BookingErrors.NoPermission)) return Redirect("NoPermission");

        if (result.IsFailure)
            return BadRequest();
        viewModel.Response = result.Value;
        return View(viewModel);
    }

    [HttpGet("Booking/{bookingId:guid}")]
    public async Task<IActionResult> Detail(Guid bookingId, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_BOOKING", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new GetBookingCommand(bookingId);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest();
        return View(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateBookingStatus([FromForm] UpdateBookingStatusRequest body,
        CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_BOOKING", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var bookingStatus = (BookingStatus)body.Status;
        ICommand command = bookingStatus switch
        {
            BookingStatus.Cancelled => new CancelBookingCommand(Guid.Parse(body.BookingId)),
            BookingStatus.Confirmed => new ConfirmBookingCommand(Guid.Parse(body.BookingId)),
            BookingStatus.Completed => new CompleteBookingCommand(Guid.Parse(body.BookingId)),
            BookingStatus.Rejected => new RejectBookingCommand(Guid.Parse(body.BookingId)),
            _ => null
        };
        if (command is null) return BadRequest();
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest();
        return Ok(new { result = "Success" });
    }

    [HttpGet("Booking/ManageBookingView")]
    public async Task<IActionResult> ManageBookingView(Guid? bookingId, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_BOOKING", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var getAllMemberCommand = new GetAllMemberCommand();
        var getAllMemberResult = await _sender.Send(getAllMemberCommand, cancellationToken);
        if (getAllMemberResult.IsFailure)
            return BadRequest();
        var getAllRestaurantCommand = new GetAllRestaurantCommand();
        var getAllRestaurantResult = await _sender.Send(getAllRestaurantCommand, cancellationToken);
        if (getAllRestaurantResult.IsFailure)
            return BadRequest();
        var getAllProductCommand = new GetAllProductCommand();
        var getAllProductResult = await _sender.Send(getAllProductCommand, cancellationToken);

        var manageBookingViewModel = new ManageBookingViewModel
        {
            //MemberList = getAllMemberResult.Value,
            RestaurantList = getAllRestaurantResult.Value,
            ProductList = getAllProductResult.Value,
            BookingModel = new BookingModel(),
            Title = "Add Reservation"
        };
        if (bookingId.HasValue)
        {
            var getBookingCommand = new GetBookingCommand(bookingId.Value);
            var bookingResult = await _sender.Send(getBookingCommand, cancellationToken);
            if (bookingResult.IsFailure)
                return Redirect("/NotFound");
            var booking = bookingResult.Value;
            manageBookingViewModel.Title = "Update Reservation";
            manageBookingViewModel.BookingModel = new BookingModel
            {
                Id = booking.Id,
                Date = booking.BookingTime.ToString("dd/MM/yyyy"),
                Time = booking.BookingTime.ToString("HH:mm"),
                FullName = booking.MemberName,
                PhoneNumber = booking.PhoneNumber ?? "",
                NumberOfPeople = booking.TotalOfPeople,
                RestaurantId = booking.RestaurantId,
                Note = booking.Note,
                LineItems = booking.LineItemResponses?.Select(l => new BookingLineItemModel
                {
                    Note = l.Note,
                    Quantity = l.Quantity,
                    ProductId = l.ProductId
                }).ToList()
            };
        }

        return View(manageBookingViewModel);
    }

    [HttpPost("Booking/Reserve")]
    public async Task<IActionResult> BookingReserve([FromForm] BookingModel model,
        CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_BOOKING", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var lineItems = model.LineItems?.Select(x => new CreateLineItem
        {
            Quantity = x.Quantity,
            Note = x.Note,
            ProductId = new ProductId(x.ProductId)
        }).ToList();
        var isCreateOrder = lineItems is { Count: > 0 };

        var bookingDateTime = model.DateAndTime.StringToDateTimeUtc(true);
        var bookingCommand = new ReserveBookingCommand(model.RestaurantId, model.NumberOfPeople,
            model.PhoneNumber, model.FullName, model.Note, bookingDateTime, !isCreateOrder, true);
        var bookingResult = await _sender.Send(bookingCommand, cancellationToken);
        if (bookingResult.IsFailure)
            return BadRequest();
        if (!isCreateOrder) return RedirectToAction("Index");
        var order = new CreateOrUpdateOrderCommand(bookingResult.Value.Member.Id.Value, null, lineItems, null, null,
            bookingResult.Value);
        var orderResult = await _sender.Send(order, cancellationToken);
        if (orderResult.IsFailure)
            return BadRequest();
        return RedirectToAction("Index");
    }

    [HttpPost("Booking/Update")]
    public async Task<IActionResult> Update([FromForm] BookingModel model, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_BOOKING", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var lineItems = model.LineItems?.Select(x => new CreateLineItem
        {
            Quantity = x.Quantity,
            Note = x.Note,
            ProductId = new ProductId(x.ProductId)
        }).ToList();

        var bookingDateTime = model.DateAndTime.StringToDateTimeUtc(true);
        if (!model.Id.HasValue)
            return BadRequest();
        var bookingCommand = new UpdateBookingCommand(model.Id.Value, model.RestaurantId, model.NumberOfPeople,
            model.PhoneNumber, model.FullName, lineItems, model.Note, bookingDateTime);
        var bookingResult = await _sender.Send(bookingCommand, cancellationToken);
        if (bookingResult.IsFailure)
            return BadRequest();
        /*if (!isCreateOrder) return RedirectToAction("Index");
        var order = new CreateOrderCommand(bookingResult.Value.Member.Id.Value, null, lineItems, null, null,
            bookingResult.Value);
        var orderResult = await _sender.Send(order, cancellationToken);
        if (orderResult.IsFailure)
            return BadRequest();*/
        return Ok("Ok");
    }
}