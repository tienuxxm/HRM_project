using Application.Abstractions.Authentication;
using Application.Abstractions.Role;
using Application.Categories.GetAll;
using Application.FileUpload;
using Application.Products.Create;
using Application.Products.Delete;
using Application.Products.GetAllPaged;
using Application.Products.GetOne;
using Application.Products.Update;
using Application.Restaurants.GetAll;
using Domain.Categories;
using Domain.Products;
using Domain.Restaurants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Backend.Models;

namespace Web.Backend.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly IRoleService _roleService;
    private readonly ISender _sender;
    private readonly IUserContext _userContext;

    public ProductsController(ISender sender, IRoleService roleService, IUserContext userContext)
    {
        _sender = sender;
        _roleService = roleService;
        _userContext = userContext;
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
            await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_MENU", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new GetProductsCommand
        {
            Page = (startValue + 10) / 10,
            PageSize = lengthValue > 0 ? lengthValue : 10
        };

        var columnOrder = column.ToString() switch
        {
            "2" => nameof(Product.ProductName),
            "3" => nameof(Product.Category.CategoryName),
            "4" => nameof(Product.Price),
            "5" => nameof(Product.AllowDelivery),
            _ => null
        };

        if (search == "allowDelivery")
        {
            command.AllowDelivery = true;
            search = "";
        }

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
                x.ProductName,
                x.Price,
                x.PriceDisplay,
                x.Id,
                x.AllowDelivery,
                x.ImageUrl,
                Category = x.CategoryResponse.CategoryName
            }).ToList(),
            pages = Math.Round((double)result.Value.TotalCount / lengthValue, MidpointRounding.AwayFromZero)
        };

        return Ok(jsonData);
    }

    // GET
    public async Task<IActionResult> Index([FromQuery] ProductPageQueryParam query, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_MENU", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new GetProductsCommand
        {
            Page = query.Page, PageSize = query.PageSize,
            SortColumn = nameof(Product.ProductName),
            SortOrder = "ASC",
            AllowDelivery = query.IsDeliveryMenu
        };
        var result = await _sender.Send(command, cancellationToken);
        var productViewModel = new ProductViewModel
        {
            ProductsResponse = result.Value,
            IsDeliveryMenu = query.IsDeliveryMenu.HasValue && query.IsDeliveryMenu.Value
        };
        return View(productViewModel);
    }

    public async Task<IActionResult> CreateProductView(CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_MENU", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new GetAllCategoryCommand();
        var result = await _sender.Send(command, cancellationToken);
        var restaurantCommand = new GetAllRestaurantCommand();
        var restaurantResult = await _sender.Send(restaurantCommand, cancellationToken);
        if (result.IsFailure)
            return NoContent();
        return View(new CreateProductViewModel
            { CategoryList = result.Value, RestaurantList = restaurantResult.Value });
    }

    public async Task<IActionResult> Detail(Guid id, CancellationToken cancellationToken)
    {
        var checkRoleExist = await _roleService.checkRoleExist(_userContext.IdentityId, "VIEW_MENU", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var getProductCommand = new GetProductCommand(new ProductId(id));
        var getProductResult = await _sender.Send(getProductCommand, cancellationToken);

        if (getProductResult.IsFailure)
            return NotFound();
        var productDetailViewModel = new ProductDetailViewModel
        {
            ProductDetail = getProductResult.Value
        };
        var getCategoriesCommand = new GetAllCategoryCommand();
        var getCategoriesResult = await _sender.Send(getCategoriesCommand, cancellationToken);
        if (getCategoriesResult.IsSuccess)
            productDetailViewModel.CategoryResponses = getCategoriesResult.Value;
        var restaurantCommand = new GetAllRestaurantCommand();
        var restaurantResult = await _sender.Send(restaurantCommand, cancellationToken);
        if (restaurantResult.IsSuccess)
            productDetailViewModel.RestaurantResponses = restaurantResult.Value;
        return View(productDetailViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateProductViewModel model,
        CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_MENU", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var imageKey = Guid.NewGuid().ToString();
        using (var ms = new MemoryStream())
        {
            await model.ImageFile.CopyToAsync(ms, cancellationToken);
            var fileCommand = new FileUploadCommand(ms, imageKey);
            await _sender.Send(fileCommand, cancellationToken);
        }

        var command =
            new CreateProductCommand(model.CategoryId, model.Name, "VND", model.Price,
                imageKey, model.AllowDelivery);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(result.Error);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromForm] ProductUpdateModel body, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_MENU", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var imageKey = Guid.NewGuid().ToString();
        if (body.ImageFile != null)
        {
            using var ms = new MemoryStream();
            await body.ImageFile.CopyToAsync(ms, cancellationToken);
            var fileCommand = new FileUploadCommand(ms, imageKey);
            await _sender.Send(fileCommand, cancellationToken);
            await ms.DisposeAsync();
        }

        var command = new UpdateProductCommand(
            new ProductId(body.Id),
            !body.CategoryId.HasValue ? null : new CategoryId(body.CategoryId.Value),
            body.RestaurantId.HasValue ? new RestaurantId(body.RestaurantId.Value) : null,
            body.ImageFile != null ? imageKey : null,
            body.ProductName,
            "VND",
            body.ProductPrice,
            body.AllowDelivery
        );

        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok(result.IsSuccess);
    }

    public IActionResult ConfirmDeletePartial(ProductResponse productResponse)
    {
        return PartialView("_ConfirmDeleteProductPartial", productResponse);
    }

    [HttpPost("/Product/Delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var checkRoleExist =
            await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_MENU", cancellationToken);
        if (!checkRoleExist.Value) return Redirect("/NoPermission");

        var command = new DeleteProductCommand(new ProductId(id));
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return NoContent();
        return RedirectToAction("Index");
    }
}