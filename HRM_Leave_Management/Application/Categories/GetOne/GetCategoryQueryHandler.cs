using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Categories;

namespace Application.Categories.GetOne;

internal sealed class GetCategoryQueryHandler : IQueryHandler<GetCategoryQuery, CategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CategoryResponse>> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
    {
        var result = await _categoryRepository
            .GetByIdAsync(new CategoryId(request.Id), cancellationToken);
        if (result is null)
            return Result.Failure<CategoryResponse>(CategoryErrors.NotFound);
        var resultDto = new CategoryResponse
        {
            Id = result.Id.Value,
            CategoryName = result.CategoryName.Value
        };
        return Result.Success(resultDto);
    }
}