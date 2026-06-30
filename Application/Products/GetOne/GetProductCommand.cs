using Application.Abstractions.Messaging;
using Domain.Products;

namespace Application.Products.GetOne;

public sealed record GetProductCommand(ProductId Id) : ICommand<ProductResponse>;