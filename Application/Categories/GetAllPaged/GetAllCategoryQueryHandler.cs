using Application.Abstractions.Messaging;
using Application.Categories.GetOne;
using Domain.Abstractions;
using Domain.Categories;

namespace Application.Categories.GetAllPaged;

internal sealed class GetAllCategoryQueryHandler : IQueryHandler<GetAllCategoryQuery, GetAllCategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetAllCategoryQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<GetAllCategoryResponse>> Handle(GetAllCategoryQuery request,
        CancellationToken cancellationToken)
    {
        var query = _categoryRepository.GetEntitiesAsQueryable().OrderBy(x => x.Index)
            .ThenBy(x => x.CategoryName);
        var listCategory = await _categoryRepository.GetAllPaged(request, query);
        var listCategoryResponse = listCategory.Data.Select(x =>
            new CategoryResponse()
            {
                Id = x.Id.Value, CategoryName = x.CategoryName.Value, Description = x.Description.Value, Index = x.Index
            }).ToList();
        return Result.Success(
            new GetAllCategoryResponse(listCategoryResponse, listCategory.TotalCount,
                listCategory.CurrentPage, listCategory.PageSize));
    }
}