using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Products;

namespace Application.Products.GetAllPaged;

public sealed record GetProductsCommand : PagedQuery<Product, ProductId>, ICommand<GetProductsResponse>
{
    public bool? AllowDelivery { get; set; }
};