using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.Members.GetOne;
using Application.Orders.Create;
using Application.Orders.GetAllPaged;
using Application.Orders.GetOrder;
using Application.Orders.SystemConfirmPayment;
using Application.Orders.UpdateOrderStatus;
using Application.Products.GetAll;
using Domain.Deliveries;
using Domain.Invoices;
using Domain.Members;
using Domain.Orders;
using Domain.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;
using Note = Domain.Deliveries.Note;
using PhoneNumber = Domain.Shared.PhoneNumber;

namespace Web.Backend.Controllers;

[Route("delivery")]
[Authorize]
public class OrderAndDeliveryController : Controller
{
    private readonly IRoleService _roleService;
    private readonly ISender _sender;
    private readonly IUserContext _userContext;

    public OrderAndDeliveryController(ISender sender, IUserContext userContext, IRoleService roleService)
    {
        _sender = sender;
        _userContext = userContext;
        _roleService = roleService;
    }

    [HttpPost("get-data")]
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
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_DELIVERY", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new GetAllOrderPagedCommand
        {
            Page = (startValue + 10) / 10,
            PageSize = lengthValue > 0 ? lengthValue : 10
        };

        var columnOrder = column.ToString() switch
        {
            "0" => nameof(Order.OrderCode),
            "1" => nameof(Order.TotalBill),
            "2" => "TotalQuantity",
            "3" => nameof(Order.CreatedDate),
            "5" => nameof(Order.Status),
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
            data = result.Value.Data.Select(x => new
            {
                x.OrderCode,
                x.TotalPrice,
                x.TotalPriceDisplay,
                x.Id,
                x.TotalQuantity,
                x.CreatedDate,
                x.CompletedDate,
                x.Status,
                x.StatusDisplay
            }).ToList(),
            pages = Math.Round((double)result.Value.TotalCount / lengthValue, MidpointRounding.AwayFromZero)
        };

        return Ok(jsonData);
    }

    public async Task<IActionResult> Index([FromQuery] PageQueryParam query, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_DELIVERY", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new GetAllOrderPagedCommand
        {
            Page = query.Page,
            PageSize = query.PageSize,
            SearchTerm = query.SearchTerm
        };

        var viewModel = new OrderAndDeliveryViewModel
        {
            SearchTerm = query.SearchTerm,
            SortColumn = query.SortColumn,
            SortOrder = query.SortOrder
        };

        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest();
        viewModel.Response = result.Value;
        return View(viewModel);
    }

    [HttpGet("Delivery/{orderId:guid}")]
    public async Task<IActionResult> Detail(Guid orderId, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_DELIVERY", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new GetOrderCommand(orderId);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        var viewModel = new OrderAndDeliveryDetailViewModel
        {
            Order = result.Value
        };
        return View(viewModel);
    }

    [HttpPut("update-order-status")]
    public async Task<IActionResult> UpdateOrderStatus(int status, Guid orderId,
        CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_DELIVERY", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new UpdateOrderStatusCommand(orderId, (OrderStatus)status);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok();
    }

    [HttpPut("confirm-order")]
    public async Task<IActionResult> ConfirmPayment(Guid id, CancellationToken cancellationToken)
    {
        var command = new SystemConfirmPaymentCommand(id);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok();
    }


    [HttpGet("ManageOrderView")]
    public async Task<IActionResult> ManageOrderView(Guid? id, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_DELIVERY", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var viewModel = new ManageOrderViewModel
        {
            Model = new OrderViewModel()
        };
        var productCommand = new GetAllProductCommand(true);
        var productResult = await _sender.Send(productCommand, cancellationToken);
        if (id.HasValue)
        {
            var orderCommand = new GetOrderCommand(id.Value);
            var orderResult = await _sender.Send(orderCommand, cancellationToken);
            if (orderResult.IsFailure)
                return Redirect("/NotFound");
            var order = orderResult.Value;
            var memberCommand = new GetMemberByIdCommand(new MemberId(order.MemberId));
            var memberResult = await _sender.Send(memberCommand, cancellationToken);
            viewModel.Model = new OrderViewModel
            {
                Note = order.Note,
                Id = id,
                CompanyAddress = order.Delivery?.CompanyAddress,
                CompanyEmail = order.Delivery?.CompanyEmail,
                CompanyName = order.Delivery?.CompanyName,
                DeliveryAddress = order.Delivery?.ReceivingAddress ?? "",
                CompanyTaxCode = order.Delivery?.CompanyTaxCode,
                MemberId = order.MemberId,
                HasRequestCutlery = order.Delivery?.HasRequestCutlery ?? false,
                HasIssueAnInvoice = order.Delivery?.HasIssueAnInvoice ?? true,
                PaymentType = order.PaymentType ?? PaymentType.Banking,
                Fullname = memberResult.IsSuccess ? memberResult.Value.FirstName : string.Empty,
                LineItems = order.LineItems.Select(l => new OrderLineItemModel
                {
                    Note = l.Note,
                    Quantity = l.Quantity,
                    ProductId = l.ProductId
                }).ToList()
            };
        }

        if (productResult.IsSuccess)
            viewModel.Products = productResult.Value;
        return View(viewModel);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromForm] OrderViewModel request,
        CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_DELIVERY", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var lineItems = request.LineItems.Select(x => new CreateLineItem
        {
            Quantity = x.Quantity,
            Note = x.Note,
            ProductId = new ProductId(x.ProductId)
        }).ToList();
        var memberCommand = new GetMemberByIdCommand(new MemberId(request.MemberId));
        var memberResult = await _sender.Send(memberCommand, cancellationToken);
        if (memberResult.IsFailure)
            return BadRequest();
        var member = memberResult.Value;
        var note = new Note(request.Note ?? "");
        var receiverName = new ReceiverName(member.FullName);
        var receiverPhone = new PhoneNumber(member.PhoneNumber);
        var receiverAddress = new ReceivingAddress(request.DeliveryAddress);
        var hasRequestCutlery = new HasRequestCutlery(request.HasRequestCutlery);
        var hasIssueAnInvoice = new HasIssueAnInvoice(request.HasIssueAnInvoice);
        var companyAddress = new CompanyAddress(request.CompanyAddress ?? "");
        var companyEmail = new CompanyEmail(request.CompanyEmail ?? "");
        var companyTaxCode = new CompanyTaxCode(request.CompanyTaxCode ?? "");
        var companyName = new CompanyName(request.CompanyName ?? "");
        var patymentType = request.PaymentType;

        var delivery = new CreateDelivery(receiverName, receiverPhone, receiverAddress, note, hasIssueAnInvoice,
            companyTaxCode, companyName, companyEmail, companyAddress, hasRequestCutlery);

        var command = new CreateOrUpdateOrderCommand(request.MemberId, request.Note, lineItems, delivery, patymentType,
            null,
            OrderType.Delivery, request.Id ?? null);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest();
        return Ok();
    }
}