using Application.Abstractions.Messaging;

namespace Application.Products.Create;

public sealed record CreateProductCommand(
    Guid CategoryId,
    string Name,
    string Currency,
    decimal Price,
    string ImageUrl,
    bool allowDelivery = false) : ICommand<Guid>;