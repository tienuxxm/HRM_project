using Application.Abstractions.Messaging;
using Application.Categories.GetOne;
using Domain.Abstractions;
using Domain.Categories;

namespace Application.Categories.GetAll;

public class GetAllCategoryCommandHandler : ICommandHandler<GetAllCategoryCommand, List<CategoryResponse>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetAllCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<List<CategoryResponse>>> Handle(GetAllCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAll(cancellationToken);
        if (categories is null)
            return Result.Failure<List<CategoryResponse>>(CategoryErrors.NotFound);
        var categoriesDto = categories.Select(x => new CategoryResponse()
        {
            Id = x.Id.Value,
            CategoryName = x.CategoryName.Value,
            Description = x.Description.Value
        }).ToList();
        return Result.Success(categoriesDto);
    }
}