using Application.Products.GetOne;
using Domain.Abstractions;

namespace Application.Products.GetAllPaged;

public sealed class GetProductsResponse : PagedList<ProductResponse>
{
    public GetProductsResponse(List<ProductResponse> items, int count, int pageNumber, int pageSize) : base(items,
        count, pageNumber, pageSize)
    {
    }
}