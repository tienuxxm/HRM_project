using Application.Abstractions.Messaging;
using Domain.Categories;
using Domain.Products;
using Domain.Restaurants;

namespace Application.Products.Update;

public record UpdateProductCommand(
    ProductId ProductId,
    CategoryId? CategoryId,
    RestaurantId? RestaurantId,
    string? ImageKey,
    string? Name,
    string? Currency,
    decimal? Amount,
    bool? allowDelivery = false
) : ICommand;