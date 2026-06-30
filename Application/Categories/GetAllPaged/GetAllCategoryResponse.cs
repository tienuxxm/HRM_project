using Application.Categories.GetOne;
using Domain.Abstractions;

namespace Application.Categories.GetAllPaged;

public sealed class GetAllCategoryResponse : PagedList<CategoryResponse>
{
    public GetAllCategoryResponse(List<CategoryResponse> items, int count, int pageNumber, int pageSize) : base(items,
        count, pageNumber, pageSize)
    {
    }
}