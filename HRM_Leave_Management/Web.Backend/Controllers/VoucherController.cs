using Application.Abstractions.Authentication;
using Application.Abstractions.AWS;
using Application.Abstractions.Role;
using Application.FileUpload;
using Application.MembershipClasses.GetAll;
using Application.Partners.GetAll;
using Application.Partners.GetOne;
using Application.Restaurants.GetAll;
using Application.Vouchers.Create;
using Application.Vouchers.Delete;
using Application.Vouchers.GetAllPaged;
using Application.Vouchers.GetOne;
using Application.Vouchers.GetVoucherDefault;
using Application.Vouchers.Update;
using Domain.Partners;
using Domain.Shared;
using Domain.Vouchers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;

namespace Web.Backend.Controllers;

[Authorize]
public class VoucherController : Controller
{
    private readonly IAwsS3Service _awsS3Services;

    private readonly IRoleService _roleService;
    private readonly ISender _sender;
    private readonly IUserContext _userContext;

    // GET
    public VoucherController(ISender sender, IAwsS3Service awsS3Services, IUserContext userContext,
        IRoleService roleService)
    {
        _sender = sender;
        _awsS3Services = awsS3Services;
        _userContext = userContext;
        _roleService = roleService;
    }

    public async Task<IActionResult> LoadData(CancellationToken cancellationToken)
    {
        Request.Form.TryGetValue("length", out var length);
        Request.Form.TryGetValue("draw", out var draw);
        Request.Form.TryGetValue("start", out var start);
        Request.Form.TryGetValue("order[0][column]", out var column);
        Request.Form.TryGetValue("order[0][dir]", out var order);
        Request.Form.TryGetValue("search[value]", out var search);
        Request.Query.TryGetValue("partnerId", out var partnerId);

        var lengthValue = int.Parse(length.ToString());
        var startValue = int.Parse(start.ToString());

        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_VOUCHER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command =
            new GetAllVoucherPagedCommand(string.IsNullOrEmpty(partnerId) ? null : Guid.Parse(partnerId.ToString()))
            {
                Page = (startValue + 10) / 10,
                PageSize = lengthValue > 0 ? lengthValue : 10
            };

        var columnOrder = column.ToString() switch
        {
            "2" => nameof(Voucher.TitleVoucher),
            "3" => "Date",
            "4" => nameof(Voucher.Point),
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
                x.Id,
                Title = x.TitleVoucher,
                x.Date,
                x.QrCodeId,
                x.Point,
                x.StatusDisplay,
                x.Status
            }),
            pages = Math.Round((double)result.Value.TotalCount / lengthValue, MidpointRounding.AwayFromZero)
        };

        return Ok(jsonData);
    }

    [HttpGet("Voucher")]
    public async Task<IActionResult> Index([FromQuery] VoucherQueryParam query, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_VOUCHER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        ViewBag.ListTitle = "The Vouchers";
        ViewBag.CurrentUrl = Request.Path + Request.QueryString.ToString();
        if (query.partnerId.HasValue) ViewBag.PartnerId = query.partnerId.Value;

        if (query.partnerId.HasValue)
        {
            var getPartnerCommand = new GetOnePartnerCommand(new PartnerId(query.partnerId.Value));
            var partner = await _sender.Send(getPartnerCommand, cancellationToken);
            if (partner.IsFailure)
                return Redirect("/NotFound");
            ViewBag.ListTitle = "Partner voucher list " + partner.Value.PartnerName;
            ViewBag.PartnerId = query.partnerId;
        }

        var command = new GetAllVoucherPagedCommand(query.partnerId)
        {
            Page = query.Page,
            PageSize = query.PageSize,
            SortColumn = nameof(Voucher.CreatedDate),
            SortOrder = "DESC"
        };
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest();
        return View(result.Value);
    }

    public async Task<IActionResult> ManageVoucherView(Guid? id, Guid? partnerId, int? voucherDefaultType,
        CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_VOUCHER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var viewModel = new ManageVoucherViewModel();
        var restaurantCommand = new GetAllRestaurantCommand();
        var restaurantResult = await _sender.Send(restaurantCommand, cancellationToken);
        var membershipClassCommand = new GetAllMembershipClassCommand();
        var membershipClasses = await _sender.Send(membershipClassCommand, cancellationToken);
        if (restaurantResult.IsSuccess) viewModel.RestaurantResponses = restaurantResult.Value;
        if (membershipClasses.IsSuccess) viewModel.MembershipClassResponses = membershipClasses.Value;

        var getPartnersCommand = new GetAllPartnerCommand();
        var partnerResult = await _sender.Send(getPartnersCommand, cancellationToken);
        if (partnerId.HasValue)
        {
            if (!partnerResult.Value.Any(x => x.Id.Equals(partnerId.Value)))
                return Redirect("/NotFound");

            viewModel.PartnerId = partnerId;
            viewModel.PartnerName = partnerResult.Value.First(x => x.Id == partnerId.Value).PartnerName;
        }

        if (id is not null)
        {
            var getVoucherCommand = new GetOneVoucherCommand(new VoucherId(id.Value));
            var voucherResult = await _sender.Send(getVoucherCommand, cancellationToken);
            if (voucherResult.IsFailure)
                return Redirect("/NotFound");
            viewModel.ManageVoucherModel = new ManageVoucherModel
            {
                PageTitle = "Cập nhật voucher",
                Id = voucherResult.Value.Id,
                Title = voucherResult.Value.TitleVoucher,
                Content = voucherResult.Value.ContentVoucher,
                Place = voucherResult.Value.Place,
                StartDate = voucherResult.Value.StartedDate.ToString(),
                VoucherImage = voucherResult.Value.ImageUrl,
                QrCodeImage = voucherResult.Value.QrCodeImageUrl,
                StartDateDefault = voucherResult.Value.StartedDate.Value,
                EndedDateDefault = voucherResult.Value.EndedDate.Value,
                EndDate = voucherResult.Value.EndedDate.ToString(),
                Point = voucherResult.Value.Point,
                PartnerId = voucherResult.Value?.PartnerId,
                Conditions = voucherResult.Value?.Conditions,
                LimitQuantity = voucherResult.Value?.LimitQuantity,
                PartnerName = voucherResult.Value?.PartnerName,
                DiscountPercent = voucherResult.Value?.DiscountPercent,
                DiscountValue = voucherResult.Value?.DiscountValue,
                MaxDiscountValue = voucherResult.Value?.MaxDiscountValue,
                MinOrderValue = voucherResult.Value?.MinOrderValue,
                Index = voucherResult.Value?.Index,
                IsUserVoucher = voucherResult?.Value?.IsUserVoucher,
                MemberIdsString = voucherResult?.Value?.Members,
                MemberClassIdsString = voucherResult?.Value?.Memberships
            };
            /*if (partnerResult.IsSuccess)
                UpdateVoucherViewModel.PartnerResponses = partnerResult.Value;*/
            return View(viewModel);
        }

        if (voucherDefaultType.HasValue)
        {
            var voucherDefaultCommand = new GetVoucherDefaultCommand((VoucherDefaultType)voucherDefaultType);
            var voucherDefault = await _sender.Send(voucherDefaultCommand, cancellationToken);
            var isBirthdateVoucher = voucherDefaultType == (int)VoucherDefaultType.MemberBirthdate;
            if (voucherDefault.IsFailure)
            {
                viewModel.ManageVoucherModel = new ManageVoucherModel
                {
                    PageTitle = isBirthdateVoucher ? "Voucher dành cho sinh nhật" : "Voucher lần đầu tiên đăng ký",
                    StartDateDefault = DateTime.Now,
                    EndedDateDefault = DateTime.Now.AddDays(1),
                    IsDefault = true,
                    VoucherDefaultType = voucherDefaultType
                };

                return View(viewModel);
            }

            viewModel.ManageVoucherModel = new ManageVoucherModel
            {
                Id = voucherDefault.Value.Id,
                PageTitle = isBirthdateVoucher ? "Voucher dành cho sinh nhật" : "Voucher lần đầu tiên đăng ký",
                IsDefault = true,
                VoucherDefaultType = voucherDefaultType,
                Title = voucherDefault.Value.TitleVoucher,
                Content = voucherDefault.Value.ContentVoucher,
                Place = voucherDefault.Value.Place,
                StartDate = voucherDefault.Value.StartedDate.ToString(),
                VoucherImage = voucherDefault.Value.ImageUrl,
                QrCodeImage = voucherDefault.Value.QrCodeImageUrl,
                StartDateDefault = voucherDefault.Value.StartedDate.Value,
                EndedDateDefault = voucherDefault.Value.EndedDate.Value,
                EndDate = voucherDefault.Value.EndedDate.ToString(),
                Point = voucherDefault.Value.Point,
                PartnerId = voucherDefault.Value?.PartnerId,
                Conditions = voucherDefault.Value?.Conditions,
                LimitQuantity = voucherDefault.Value?.LimitQuantity,
                PartnerName = voucherDefault.Value?.PartnerName,
                DiscountPercent = voucherDefault.Value?.DiscountPercent,
                DiscountValue = voucherDefault.Value?.DiscountValue,
                MaxDiscountValue = voucherDefault.Value?.MaxDiscountValue,
                MinOrderValue = voucherDefault.Value?.MinOrderValue,
                Index = voucherDefault.Value?.Index
            };

            return View(viewModel);
        }

        viewModel.ManageVoucherModel = new ManageVoucherModel
        {
            PageTitle = "Add voucher",
            StartDateDefault = DateTime.Now,
            EndedDateDefault = DateTime.Now.AddDays(1)
        };


        /*if (partnerResult.IsSuccess)
                createVoucherViewModel.PartnerResponses = partnerResult.Value;*/
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] ManageVoucherModel body, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_VOUCHER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var voucherImageKey = Guid.NewGuid().ToString();
        using (var ms = new MemoryStream())
        {
            await body.VoucherImageUrl.CopyToAsync(ms, cancellationToken);
            var fileCommand = new FileUploadCommand(ms, voucherImageKey);
            await _sender.Send(fileCommand, cancellationToken);
        }

        var command = new CreateVoucherCommand(body.Title, voucherImageKey, body.StartDateUtc,
            body.EndDateUtc,
            body.Place, body.Point, body.PartnerId, body.Content, body.Conditions, body.LimitQuantity,
            body.DiscountValue, body.DiscountPercent, body.MinOrderValue, body.MaxDiscountValue, body.Index,
            body.IsDefault, body.VoucherDefaultType.HasValue ? (VoucherDefaultType)body.VoucherDefaultType : null,
            body.MemberIds, body.MemberClassIds);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return Redirect($"/Voucher?partnerId{body.PartnerId.ToString()}");
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Update([FromForm] ManageVoucherModel body, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_VOUCHER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        if (body.Id.HasValue)
        {
            var voucherCommand = new GetOneVoucherCommand(new VoucherId(body.Id.Value));
            var voucher = await _sender.Send(voucherCommand, cancellationToken);
            if (voucher.IsFailure)
                return NoContent();

            var voucherImageKey = body.VoucherImageUrl is not null ? Guid.NewGuid().ToString() : null;

            if (body.VoucherImageUrl is not null)
                using (var ms = new MemoryStream())
                {
                    await body.VoucherImageUrl.CopyToAsync(ms, cancellationToken);
                    var fileCommand = new FileUploadCommand(ms, voucherImageKey);
                    await _sender.Send(fileCommand, cancellationToken);
                }


            var command = new UpdateVoucherCommand(
                new VoucherId(body.Id.Value),
                new TitleVoucher(body.Title),
                voucherImageKey != null ? new ImageUrl(voucherImageKey) : null,
                body.StartDateUtc,
                body.EndDateUtc,
                string.IsNullOrEmpty(body.Place) ? null : new Place(body.Place),
                body.Point,
                null,
                string.IsNullOrEmpty(body.Content) ? null : new ContentVoucher(body.Content),
                string.IsNullOrEmpty(body.Conditions) ? null : new Conditions(body.Conditions),
                body.LimitQuantity,
                body.DiscountValue,
                body.DiscountPercent,
                body.MinOrderValue,
                body.MaxDiscountValue,
                body.Index,
                body.VoucherDefaultType.HasValue ? (VoucherDefaultType)body.VoucherDefaultType.Value : null
            );
            var result = await _sender.Send(command, cancellationToken);
            if (result.IsFailure)
                return NoContent();
        }
        else
        {
            return NoContent();
        }

        return RedirectToAction("Index");
    }

    [HttpPost("/Voucher/Delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, string? url, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_VOUCHER", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new DeleteVoucherCommand(new VoucherId(id));
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess) return string.IsNullOrEmpty(url) ? RedirectToAction("Index") : Redirect(url);

        return NoContent();
    }
}