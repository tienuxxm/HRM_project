using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Products;

namespace Application.Products.Delete;

public record DeleteProductCommand(ProductId ProductId) : ICommand<BooleanResponse>;