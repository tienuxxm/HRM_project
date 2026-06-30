using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Categories;
using Domain.Partners;
using Domain.Products;
using Domain.Shared;
using Domain.Vouchers;

namespace Application.Categories.Update;

internal sealed class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand, Category>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Category>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(new CategoryId(request.id));
        if (category is null)
        {
            return Result.Failure<Category>(CategoryErrors.NotFound);
        }

        category.Update(
            new CategoryName(request.Name),
            new Description(request.Description),
            request.Index
        );

        _categoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return category;
    }
}